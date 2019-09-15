using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider
{
    // 碰撞检测部分
    internal partial class QuadtreeNode
    {
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
