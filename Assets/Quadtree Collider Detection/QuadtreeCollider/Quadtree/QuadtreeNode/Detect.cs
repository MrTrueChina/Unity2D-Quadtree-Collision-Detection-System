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
            // 如果指定碰撞器不可能与当前节点内的任何碰撞器碰撞，返回空列表
            if (!PossibleCollisions(collider))
            {
                return new List<QuadtreeCollider>();
            }

            // 如果有子节点，从子节点中寻找发生碰撞的碰撞器
            if (HaveChildren())
            {
                return GetCollidersInCollisionFromChildren(collider);
            }

            // 没有子节点，从当前节点中寻找发生碰撞的碰撞器
            return GetCollidersInCollisionFromSelf(collider);
        }

        /// <summary>
        /// 检测指定碰撞器是否有可能与当前节点内的任何碰撞器发生碰撞
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        private bool PossibleCollisions(QuadtreeCollider collider)
        {
            // 如果节点区域到碰撞器的距离小于等于节点最大检测半径和碰撞器最大检测半径之和，则说明节点中可能有碰撞器能够与传入的碰撞器发生碰撞
            return _area.DistanceToPoint(collider.Position) <= _maxRadius + collider.MaxRadius;
        }

        /// <summary>
        /// 从子节点中获取与指定碰撞器发生碰撞的碰撞器
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        private List<QuadtreeCollider> GetCollidersInCollisionFromChildren(QuadtreeCollider collider)
        {
            List<QuadtreeCollider> colliders = new List<QuadtreeCollider>();

            // 遍历子节点进行碰撞检测并保存发生碰撞的碰撞器
            foreach (QuadtreeNode child in _children)
            {
                colliders.AddRange(child.GetCollidersInCollision(collider));
            }

            return colliders;
        }

        /// <summary>
        /// 从当前节点中获取与指定碰撞器发生碰撞的碰撞器
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        private List<QuadtreeCollider> GetCollidersInCollisionFromSelf(QuadtreeCollider collider)
        {
            List<QuadtreeCollider> colliders = new List<QuadtreeCollider>();

            // 遍历所有碰撞器，如果与指定碰撞器发生碰撞则记录到列表里
            foreach (QuadtreeCollider currentCollider in _colliders)
            {
                if (currentCollider.IsCollitionToCollider(collider))
                {
                    colliders.Add(currentCollider);
                }
            }

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
