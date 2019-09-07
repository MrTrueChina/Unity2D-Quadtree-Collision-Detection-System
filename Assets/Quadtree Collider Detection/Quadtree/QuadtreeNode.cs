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
        /// 默认最大检测半径
        /// </summary>
        private const float DEFAULT_MAX_RADIUS = Mathf.NegativeInfinity; // 默认最大检测半径为负无穷，这样无论检测器半径多大都不会判断为可能发生碰撞，减少无意义的向子节点递归

        /// <summary>
        /// 一个节点里的碰撞器数量上限，超过上限后进行分割
        /// </summary>
        private static int maxCollidersumber = 10; //TODO：分割数量需要引入配置
        /// <summary>
        /// 单个节点的最短边的最小长度，当任意一个边的长度小于这个长度时，无论碰撞器数量，不再进行分割
        /// </summary>
        private static float minSideLendth = 10; // 这个值用于应对过度分割导致树深度过大性能反而下降的情况，同时可以避免大量碰撞器位置完全相同导致的无限分割
        //TODO：最短边长需要引入配置

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
        private float _maxRadius = DEFAULT_MAX_RADIUS;

        /// <summary>
        /// 根节点的构造方法，只有区域没有父节点。根节点
        /// </summary>
        /// <param name="area"></param>
        internal QuadtreeNode(Rect area)
        {
            _area = area;
        }

        /// <summary>
        /// 根节点之外的节点的构造方法
        /// </summary>
        /// <param name="area"></param>
        /// <param name="parent"></param>
        internal QuadtreeNode(Rect area, QuadtreeNode parent) : this(area)
        {
            _parent = parent;
        }

        /// <summary>
        /// 直接存入子节点的构造方法，用于逆向生长
        /// </summary>
        /// <param name="area"></param>
        /// <param name="children"></param>
        internal QuadtreeNode(Rect area, List<QuadtreeNode> children, int mainNodeIndex) : this(area)
        {
            _children = children;

            _maxRadius = children[mainNodeIndex]._maxRadius;
        }

        /// <summary>
        /// 向四叉树中存入碰撞器
        /// </summary>
        /// <param name="collider">存入的碰撞器</param>
        /// <returns> 如果成功存入，返回 true </returns>
        internal bool AddCollider(QuadtreeCollider collider)
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
            foreach (QuadtreeNode child in _children)
                if (child.AddCollider(collider))
                    return true;
            return false;
        }

        private void AddColliderIntoSelf(QuadtreeCollider collider)
        {
            _colliders.Add(collider);

            if (NeedSplit())
                Split();
        }

        private bool NeedSplit()
        {
            return _colliders.Count > maxCollidersumber && _area.height > minSideLendth && _area.width > minSideLendth;
        }

        private void Split()
        {
            /*
             *  清除掉不在自己区域内的碰撞器，防止下发碰撞器失败
             *  分割处子节点并下发碰撞器
             *  把清除掉的那些碰撞器重新存入四叉树
             */
            List<QuadtreeCollider> outOfAreaColliders = GetAndRemoveCollidersOutOfField();
            DoSplite();
            ResetCollidersIntoQuadtree(outOfAreaColliders);
        }

        private List<QuadtreeCollider> GetAndRemoveCollidersOutOfField()
        {
            List<QuadtreeCollider> outOfFieldCollider = new List<QuadtreeCollider>();

            for (int i = _colliders.Count - 1; i >= 0; i--)
                if (!_area.Contains(_colliders[i].position))
                {
                    outOfFieldCollider.Add(_colliders[i]);
                    RemoveSelfColliderOnSplit(i);
                }

            return outOfFieldCollider;
        }

        private void RemoveSelfColliderOnSplit(int index)
        {
            _colliders.RemoveAt(index);
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

            _children = new List<QuadtreeNode>
            {
                new QuadtreeNode(new Rect(_area.x + halfWidth, _area.y + halfHeight, _area.width - halfWidth, _area.height - halfHeight), this), // 右上子节点
                new QuadtreeNode(new Rect(_area.x + halfWidth, _area.y, _area.width - halfWidth, halfHeight), this), // 右下子节点
                new QuadtreeNode(new Rect(_area.x, _area.y, halfWidth, halfHeight), this), // 左下子节点
                new QuadtreeNode(new Rect(_area.x, _area.y + halfHeight, halfWidth, _area.height - halfHeight), this) // 左上子节点
            };
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
            if (!RemoveColliderByPosition(collider)) // 首先根据位置移除碰撞器，但有时候碰撞器移出了所在节点的范围，就会发生找不到节点无法移除的情况
                RemoveColliderFromAllNodes(collider); // 此时使用全节点遍历移除
        }

        private bool RemoveColliderByPosition(QuadtreeCollider collider)
        {
            if (HaveChildren())
                return RemoveColliderFromChildrenByPosition(collider);

            return RemoveColliderFromSelfByPosition(collider);
        }

        private bool RemoveColliderFromChildrenByPosition(QuadtreeCollider collider)
        {
            foreach (QuadtreeNode child in _children)
                if (child.RemoveColliderByPosition(collider))
                    return true;

            return false;
        }

        private bool RemoveColliderFromSelfByPosition(QuadtreeCollider collider)
        {
            if (_area.Contains(collider.position))
                return RemoveColliderFromSelf(collider);

            return false;
        }

        private bool RemoveColliderFromSelf(QuadtreeCollider collider)
        {
            return _colliders.Remove(collider);
        }

        private bool RemoveColliderFromAllNodes(QuadtreeCollider collider)
        {
            if (HaveChildren())
                return RemoveColliderFromChildrenAndAllNodes(collider);

            return RemoveColliderFromSelf(collider);
        }

        private bool RemoveColliderFromChildrenAndAllNodes(QuadtreeCollider collider)
        {
            foreach (QuadtreeNode child in _children)
                if (child.RemoveColliderFromAllNodes(collider))
                    return true;

            return false;
        }

        internal void Update()
        {
            UpdatePosition();
            UpdateMaxRadius();
        }

        private void UpdatePosition()
        {
            if (HaveChildren())
                UpdateChildrenPosition();
            else
                UpdateSelfPosition();
        }

        private void UpdateChildrenPosition()
        {
            foreach (QuadtreeNode child in _children)
                child.UpdatePosition();
        }

        private void UpdateSelfPosition()
        {
            List<QuadtreeCollider> outOfAreaColliders = GetAndRemoveCollidersOutOfField();
            ResetCollidersIntoQuadtree(outOfAreaColliders);
        }

        private float UpdateMaxRadius()
        {
            if (HaveChildren())
                return _maxRadius = UpdateChildrenMaxRadius();
            else
                return _maxRadius = UpdateSelfMaxRadius();
        }

        private float UpdateChildrenMaxRadius()
        {
            _maxRadius = DEFAULT_MAX_RADIUS;

            foreach (QuadtreeNode child in _children)
                _maxRadius = Mathf.Max(_maxRadius, child.UpdateMaxRadius());

            return _maxRadius;
        }

        private float UpdateSelfMaxRadius()
        {
            _maxRadius = DEFAULT_MAX_RADIUS;

            foreach (QuadtreeCollider collider in _colliders)
                if (collider.maxRadius > _maxRadius)
                    _maxRadius = collider.maxRadius;

            return _maxRadius;
        }

        /// <summary>
        /// 获取与指定碰撞器发生碰撞的碰撞器
        /// </summary>
        /// <param name="collider">用于检测碰撞的碰撞器</param>
        /// <returns></returns>
        internal List<QuadtreeCollider> GetCollidersInCollision(QuadtreeCollider collider)
        {
            if (!PossibleCollisions(collider))
                return new List<QuadtreeCollider>();

            if (HaveChildren())
                return GetCollidersInCollisionFromChildren(collider);

            return GetCollidersInCollisionFromSelf(collider);
        }

        private bool PossibleCollisions(QuadtreeCollider collider)
        {
            return _area.DistanceToPoint(collider.position) <= _maxRadius + collider.maxRadius; // 如果节点区域到碰撞器的距离小于等于节点最大检测半径和碰撞器最大检测半径之和，则说明节点中可能有碰撞器能够与传入的碰撞器发生碰撞
        }

        private List<QuadtreeCollider> GetCollidersInCollisionFromChildren(QuadtreeCollider collider)
        {
            List<QuadtreeCollider> colliders = new List<QuadtreeCollider>();

            foreach (QuadtreeNode child in _children)
                colliders.AddRange(child.GetCollidersInCollision(collider));

            return colliders;
        }

        private List<QuadtreeCollider> GetCollidersInCollisionFromSelf(QuadtreeCollider collider)
        {
            List<QuadtreeCollider> colliders = new List<QuadtreeCollider>();

            foreach (QuadtreeCollider currentCollider in _colliders)
                if (currentCollider.IsCollitionToCollider(collider))
                    colliders.Add(currentCollider);

            return colliders;
        }
    }

    /// <summary>
    /// Rect扩展方法类
    /// </summary>
    static partial class RectExtension
    {
        /// <summary>
        /// 计算与指定Vector2的距离，如果指定Vector2在Rect范围内则返回0
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static float DistanceToPoint(this Rect rect, Vector2 point)
        {
            float xDistance = Mathf.Max(0, point.x - rect.xMax, rect.xMin - point.x);
            float yDistance = Mathf.Max(0, point.y - rect.yMax, rect.yMin - point.y);
            return Mathf.Sqrt(xDistance * xDistance + yDistance * yDistance);
        }
    }
}
