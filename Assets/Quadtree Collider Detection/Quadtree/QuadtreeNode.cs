using System;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider
{
    /// <summary>
    /// 四叉树节点
    /// </summary>
    internal class QuadtreeNode
    {
        /// <summary>
        /// 右上子节点的索引
        /// </summary>
        private const int RIGHT_TOP_CHILD_INDEX = 0;
        /// <summary>
        /// 右下子节点的索引
        /// </summary>
        private const int RIGHT_BOTTOM_CHILD_INDEX = 1;
        /// <summary>
        /// 左下子节点的索引
        /// </summary>
        private const int LEFT_BOTTOM_CHILD_INDEX = 2;
        /// <summary>
        /// 左上子节点的索引
        /// </summary>
        private const int LEFT_TOP_CHILD_INDEX = 3;

        /// <summary>
        /// 一个节点里的碰撞器数量上限，超过上限后进行分割
        /// </summary>
        private static int maxCollidersumber = 10;
        /// <summary>
        /// 单个节点的最短边的最小长度，当任意一个边的长度小于这个长度时，无论碰撞器数量，不再进行分割
        /// </summary>
        private static float minSideLendth = 10; // 这个值用于应对过度分割导致树深度过大性能反而下降的情况，同时可以避免大量碰撞器位置完全相同导致的无限分割

        /// <summary>
        /// 父节点
        /// </summary>
        private QuadtreeNode _parent = null;
        /// <summary>
        /// 四叉树节点所拥有的区域
        /// </summary>
        private Rect _area = default;
        /// <summary>
        /// 这个节点所拥有的碰撞器
        /// </summary>
        private List<QuadtreeCollider> _colliders = new List<QuadtreeCollider>();
        /// <summary>
        /// 这个节点所拥有的的子节点
        /// </summary>
        private List<QuadtreeNode> _children = null;
        /// <summary>
        /// 这个节点所拥有的所有碰撞器中，需要检测半径最长的碰撞器的检测半径
        /// </summary>
        private float _maxRadius = Mathf.NegativeInfinity;
        /// <summary>
        /// 这个节点范围内所有的碰撞器的数量
        /// </summary>
        private int _collidersNumber = 0;

        /// <summary>
        /// 根节点的构造方法，只有区域没有父节点。根节点
        /// </summary>
        /// <param name="area"></param>
        internal QuadtreeNode(Rect area)
        {
            Setup(area);
        }

        internal void Setup(Rect area)
        {
            _area = area;
            _parent = null;
        }

        /// <summary>
        /// 根节点之外的节点的构造方法
        /// </summary>
        /// <param name="area"></param>
        /// <param name="parent"></param>
        internal QuadtreeNode(Rect area, QuadtreeNode parent)
        {
            Setup(area, parent);
        }

        internal void Setup(Rect area, QuadtreeNode parent)
        {
            _area = area;
            _parent = parent;
        }

        /// <summary>
        /// 直接存入子节点的构造方法，用于逆向生长
        /// </summary>
        /// <param name="area"></param>
        /// <param name="children"></param>
        internal QuadtreeNode(Rect area, List<QuadtreeNode> children, int mainNodeIndex)
        {
            Setup(area, children, mainNodeIndex);
        }

        internal void Setup(Rect area, List<QuadtreeNode> children, int mainNodeIndex)
        {
            _area = area;
            _children = children;

            _collidersNumber = children[mainNodeIndex]._collidersNumber;
            _maxRadius = children[mainNodeIndex]._maxRadius;
        }

        /// <summary>
        /// 向四叉树中存入碰撞器
        /// </summary>
        /// <param name="collider">存入的碰撞器</param>
        /// <returns> 如果成功存入，返回 true </returns>
        internal bool AddCollider(QuadtreeCollider collider) // TODO：缺少节点范围内碰撞器数量维护，缺少分割
        {
            if (!_area.Contains(collider.position))
                return false;

            if (HaveChildren())
                return AddColliderIntoChildren(collider);

            AddColliderIntoSelf(collider);
            return true;
        }

        private bool HaveChildren()
        {
            return _children != null; // 子节点List只在创建子节点时才会创建，判断是不是null就能判断有没有子节点
        }

        private bool AddColliderIntoChildren(QuadtreeCollider collider)
        {
            //TODO：向子节点添加碰撞器
            throw new NotImplementedException();
        }

        private void AddColliderIntoSelf(QuadtreeCollider collider)
        {
            _colliders.Add(collider);

            UpdateMaxRadiusOnSetCollider(collider);

            if (NeedSplit())
                Split();
        }

        private void UpdateMaxRadiusOnSetCollider(QuadtreeCollider collider)
        {
            if (collider.maxRadius > _maxRadius)
            {
                _maxRadius = collider.maxRadius;
                UpwardUpdateMaxRadius();
            }
        }

        private void UpwardUpdateMaxRadius()
        {
            //TODO：向上更新最大半径
            throw new NotImplementedException();
        }

        private bool NeedSplit()
        {
            return _area.height > minSideLendth && _area.width > minSideLendth && _colliders.Count > maxCollidersumber;
        }

        private void Split()
        {
            /*
             *  清除掉不在自己区域内的碰撞器，防止下发碰撞器失败
             *  分割处子节点并下发碰撞器
             *  把清除掉的那些碰撞器重新存入四叉树
             */
            List<QuadtreeCollider> outOfFieldColliders = GetAndRemoveCollidersOutOfField();
            DoSplite();
            ResetCollidersIntoQuadtree(outOfFieldColliders);
        }

        private List<QuadtreeCollider> GetAndRemoveCollidersOutOfField()
        {
            List<QuadtreeCollider> outOfFieldCollider = new List<QuadtreeCollider>();

            for (int i = _colliders.Count; i >= 0; i--)
                if (!_area.Contains(_colliders[i].position))
                {
                    outOfFieldCollider.Add(_colliders[i]);
                    _colliders.RemoveAt(i);
                }

            return outOfFieldCollider;
        }

        private void DoSplite()
        {
            CreateChildren();
            SetAllColliderIntoChindren();
        }

        private void CreateChildren()
        {
            float halfWidth = _area.width / 2; // 为了防止float的乘除运算误差，一次运算求出宽高的一半，子节点的宽高使用加减运算获得
            float halfHeight = _area.height / 2; // 误差的来源是浮点数的储存方式，除非出现新的储存方式，否则误差将作为标准现象保留下去

            _children = new List<QuadtreeNode>();

            _children.Add(QuadtreeNodePool.Get(new Rect(_area.x + halfWidth, _area.y + halfHeight, _area.width - halfWidth, _area.height - halfHeight), this)); // 右上子节点
            _children.Add(QuadtreeNodePool.Get(new Rect(_area.x + halfWidth, _area.y, _area.width - halfWidth, halfHeight), this)); // 右下子节点
            _children.Add(QuadtreeNodePool.Get(new Rect(_area.x, _area.y, halfWidth, halfHeight), this)); // 左下子节点
            _children.Add(QuadtreeNodePool.Get(new Rect(_area.x, _area.y + halfHeight, halfWidth, _area.height - halfHeight), this)); // 左上子节点
        }

        private void SetAllColliderIntoChindren()
        {
            foreach (QuadtreeCollider collider in _colliders)
                AddColliderIntoChildren(collider);

            _colliders.Clear();
        }

        private void ResetCollidersIntoQuadtree(List<QuadtreeCollider> outOfFieldColliders)
        {
            foreach (QuadtreeCollider collider in outOfFieldColliders)
                Quadtree.AddCollider(collider); // 直接通过包装类从根节点存入
        }

        internal void RemoveCollider(QuadtreeCollider collider)
        {
            //TODO：移除碰撞器
            //TODO：分为按区域移除和全遍历移除，有时候要删除的碰撞器已经不在原来的节点的区域里了
            //TODO：向上递归更新碰撞器数量
            //TODO：对节点合并的检测和进行
            throw new NotImplementedException();
        }

        internal void Update()
        {
            //TODO：更新
            throw new NotImplementedException();
        }

        //TODO：检测

        /// <summary>
        /// 清理节点中难复用的值，用于入池
        /// </summary>
        internal void PutIntoPool()
        {
            if (HaveChildren())
                PutChildrenIntoPool();

            PutSelfIntPool();
        }

        private void PutChildrenIntoPool()
        {
            foreach (QuadtreeNode child in _children)
                child.PutIntoPool();
        }

        private void PutSelfIntPool()
        {
            Clear();
            QuadtreeNodePool.Put(this);
        }

        private void Clear()
        {
            _parent = null;
            _area = default;

            _colliders.Clear();

            _children.Clear();
            _children = null;

            _maxRadius = Mathf.NegativeInfinity;
            _collidersNumber = 0;
        }
    }
}
