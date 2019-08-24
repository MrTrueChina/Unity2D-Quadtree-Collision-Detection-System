using System;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider.One
{
    /// <summary>
    /// 四叉树节点
    /// </summary>
    public class QuadtreeNode
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
        /// 根节点
        /// </summary>
        private QuadtreeNode _root;
        /// <summary>
        /// 父节点
        /// </summary>
        private QuadtreeNode _parent;
        /// <summary>
        /// 四叉树节点所拥有的区域
        /// </summary>
        private Rect _area;
        /// <summary>
        /// 这个节点所拥有的碰撞器
        /// </summary>
        private List<QuadtreeCollider> _colliders = new List<QuadtreeCollider>();
        /// <summary>
        /// 这个节点所拥有的的子节点
        /// </summary>
        private List<QuadtreeNode> _children;
        /// <summary>
        /// 这个节点所拥有的所有碰撞器中，需要检测半径最长的碰撞器的检测半径
        /// </summary>
        private float _maxRadius = Mathf.NegativeInfinity;

        /// <summary>
        /// 根节点的构造方法，只有区域没有父节点。根节点
        /// </summary>
        /// <param name="field"></param>
        public QuadtreeNode(Rect field)
        {
            _area = field;
            _root = this;
            _parent = null;
        }
        /// <summary>
        /// 根节点之外的节点的构造方法
        /// </summary>
        /// <param name="field"></param>
        /// <param name="root"></param>
        /// <param name="parent"></param>
        public QuadtreeNode(Rect field, QuadtreeNode root, QuadtreeNode parent)
        {
            _area = field;
            _root = root;
            _parent = parent;
        }

        /// <summary>
        /// 向四叉树中存入碰撞器
        /// </summary>
        /// <param name="collider">存入的碰撞器</param>
        /// <returns> 如果成功存入，返回 true </returns>
        public bool AddCollider(QuadtreeCollider collider)
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

            UpdateMaxRadiusOnSetCollider();
            //TODO： 有讨论空间，是否需要在存入时更新半径？
            /*
             *  由于现阶段Unity的特点，场景中的物体以帧为单位改变，即使是Unity自己的碰撞器也是以帧为单位进行检测的，一帧一次检测，如果可以控制更新和检测的顺序，一帧更新一次就足以满足检测的需要
             *  碰撞器的存入和移除显然不是一帧一次，在特殊情况下，碰撞器一帧可以发生几十几百次甚至更高，如此高频的操作是否有必要进行半径的更新？
             */
            //TODO：如何控制所有检测和所有更新的顺序？
            //TODO：如果顺序可以控制，是否可以将分割修改到更新时统一判断进行？

            if (NeedSplit())
                Split();
        }

        private void UpdateMaxRadiusOnSetCollider()
        {
            float newMaxRadius = Mathf.NegativeInfinity;

            foreach (QuadtreeCollider collider in _colliders)
                if (collider.maxRadius > newMaxRadius)
                    newMaxRadius = collider.maxRadius;

            if (newMaxRadius != _maxRadius)
            {
                _maxRadius = newMaxRadius;
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

        private void DoSplite() // TODO：需要单元测试
        {
            CreateChildren();
            SetAllColliderIntoChindren();
        }

        private void CreateChildren()
        {
            float halfWidth = _area.width / 2; // 为了防止float的乘除运算误差，一次运算求出宽高的一半，子节点的宽高使用加减运算获得
            float halfHeight = _area.height / 2; // 误差的来源是浮点数的储存方式，除非出现新的储存方式，否则误差将作为标准现象保留下去

            _children = new List<QuadtreeNode>();

            _children.Add(new QuadtreeNode(new Rect(_area.x + halfWidth, _area.y + halfHeight, _area.width - halfWidth, _area.height - halfHeight), _root, this)); // 右上子节点
            _children.Add(new QuadtreeNode(new Rect(_area.x + halfWidth, _area.y, _area.width - halfWidth, halfHeight), _root, this)); // 右下子节点
            _children.Add(new QuadtreeNode(new Rect(_area.x, _area.y, halfWidth, halfHeight), _root, this)); // 左下子节点
            _children.Add(new QuadtreeNode(new Rect(_area.x, _area.y + halfHeight, halfWidth, _area.height - halfHeight), _root, this)); // 左上子节点
        }

        private void SetAllColliderIntoChindren()
        {
            foreach (QuadtreeCollider collider in _colliders)
                AddColliderIntoChildren(collider);

            _colliders = null; // 树枝节点不保存碰撞器也不会变回树梢节点，这个List没用了，改null节省内存
        }

        private void ResetCollidersIntoQuadtree(List<QuadtreeCollider> outOfFieldColliders)
        {
            foreach (QuadtreeCollider collider in outOfFieldColliders)
                _root.AddCollider(collider);
        }
        //TODO：添加碰撞器
        //TODO：假设两个节点互相在对方区域有碰撞器，一个节点分割时需要先更新节点将超出区域的碰撞器剔除，此时另一个节点被触发分割，之后这个节点的更新又导致前一个节点的更新，如何处理

        //TODO：移除碰撞器
        //TODO：分为按区域移除和全遍历移除，有时候要删除的碰撞器已经不在原来的节点的区域里了

        //TODO：更新

        //TODO：检测
    }
}
