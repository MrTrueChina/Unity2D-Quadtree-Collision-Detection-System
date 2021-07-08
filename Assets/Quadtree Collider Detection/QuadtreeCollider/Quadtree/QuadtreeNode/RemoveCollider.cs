using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider
{
    // 从树中移除碰撞器的部分
    internal partial class QuadtreeNode
    {
        /// <summary>
        /// 从树中移除碰撞器
        /// </summary>
        /// <param name="collider"></param>
        internal void RemoveCollider(QuadtreeCollider collider)
        {
            // 根据位置移除碰撞器，但有时候碰撞器移出了所在节点的范围，就会发生找不到节点无法移除的情况
            if (!RemoveColliderByPosition(collider))
            {
                // 按照位置移除失败，使用全节点遍历移除}
                RemoveColliderFromAllNodes(collider);
            }
        }

        /// <summary>
        /// 基于碰撞器位置移除碰撞器
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        private bool RemoveColliderByPosition(QuadtreeCollider collider)
        {
            // 有子节点，通知子节点移除碰撞器
            if (HaveChildren())
            {
                return RemoveColliderFromChildrenByPosition(collider);
            }

            // 没有子节点，从当前节点移除碰撞器
            return RemoveColliderFromSelfByPosition(collider);
        }

        /// <summary>
        /// 从子节点中基于碰撞器位置移除碰撞器
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        private bool RemoveColliderFromChildrenByPosition(QuadtreeCollider collider)
        {
            // 遍历所有子节点并进行移除，有移除成功的则返回移除成功
            foreach (QuadtreeNode child in _children)
            {
                if (child.RemoveColliderByPosition(collider))
                {
                    return true;
                }
            }

            // 所有子节点都移除失败，返回移除失败
            return false;
        }

        /// <summary>
        /// 从当前节点中基于碰撞器位置移除碰撞器
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        private bool RemoveColliderFromSelfByPosition(QuadtreeCollider collider)
        {
            // 如果碰撞器在当前节点范围内则进行移除并返回移除结果
            if (_area.Contains(collider.Position))
            {
                return RemoveColliderFromSelf(collider);
            }

            // 不在范围内返回移除失败
            return false;
        }

        /// <summary>
        /// 从当前节点中移除指定碰撞器
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        private bool RemoveColliderFromSelf(QuadtreeCollider collider)
        {
            return _colliders.Remove(collider);
        }

        /// <summary>
        /// 从所有节点中移除指定碰撞器
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        private bool RemoveColliderFromAllNodes(QuadtreeCollider collider)
        {
            // 有子节点，通知子节点移除指定碰撞器
            if (HaveChildren())
            {
                return RemoveColliderFromChildrenAndAllNodes(collider);
            }

            // 没有子节点，移除当前节点内的指定碰撞器
            return RemoveColliderFromSelf(collider);
        }

        /// <summary>
        /// 从子节点中移除指定的碰撞器
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        private bool RemoveColliderFromChildrenAndAllNodes(QuadtreeCollider collider)
        {
            // 通知所有子节点移除碰撞器，移除成功则返回成功
            foreach (QuadtreeNode child in _children)
            {
                if (child.RemoveColliderFromAllNodes(collider))
                {
                    return true;
                }
            }

            // 所有子节点都移除失败，返回移除失败
            return false;
        }
    }
}
