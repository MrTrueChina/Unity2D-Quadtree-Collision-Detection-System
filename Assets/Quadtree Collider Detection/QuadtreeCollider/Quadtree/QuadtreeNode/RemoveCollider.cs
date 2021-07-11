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
        internal OperationResult RemoveCollider(QuadtreeCollider collider)
        {
            // 根据位置移除碰撞器
            OperationResult result = RemoveColliderByPosition(collider);

            // 可能移除时碰撞器移出了所在节点的范围，会发生找不到节点无法移除的情况，此时需要遍历全树移除
            if (!result.Success)
            {
                // 按照位置移除失败，使用全节点遍历移除，并以全节点移除的结果为移除操作的结果
                result = RemoveColliderFromAllNodes(collider);
            }

            return result;
        }

        /// <summary>
        /// 基于碰撞器位置移除碰撞器
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        private OperationResult RemoveColliderByPosition(QuadtreeCollider collider)
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
        private OperationResult RemoveColliderFromChildrenByPosition(QuadtreeCollider collider)
        {
            // 遍历所有子节点并进行移除，有移除成功的则返回子节点的移除结果
            foreach (QuadtreeNode child in children)
            {
                OperationResult result = child.RemoveColliderByPosition(collider);
                if (result.Success)
                {
                    return result;
                }
            }

            // 所有子节点都移除失败，返回移除失败
            return new OperationResult(false);
            // FIXME：移除失败应该不用更新映射表，但还是确认下比较安全
        }

        /// <summary>
        /// 从当前节点中基于碰撞器位置移除碰撞器
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        private OperationResult RemoveColliderFromSelfByPosition(QuadtreeCollider collider)
        {
            // 如果碰撞器在当前节点范围内则进行移除并返回移除结果
            if (area.Contains(collider.Position))
            {
                return RemoveColliderFromSelf(collider);
            }

            // 不在范围内返回移除失败
            return new OperationResult(false);
            // FIXME：移除失败应该不用更新映射表，但还是确认下比较安全
        }

        /// <summary>
        /// 从当前节点中移除指定碰撞器
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        private OperationResult RemoveColliderFromSelf(QuadtreeCollider collider)
        {
            bool listResult = colliders.Remove(collider);

            if (listResult)
            {
                return new OperationResult(true);
                // FIXME：需要更新映射表
            }
            else
            {
                return new OperationResult(false);
                // FIXME：移除失败应该不用更新映射表，但还是确认下比较安全
            }
        }

        /// <summary>
        /// 从所有节点中移除指定碰撞器
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        private OperationResult RemoveColliderFromAllNodes(QuadtreeCollider collider)
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
        private OperationResult RemoveColliderFromChildrenAndAllNodes(QuadtreeCollider collider)
        {
            // 通知所有子节点移除碰撞器，移除成功则返回操作结果
            foreach (QuadtreeNode child in children)
            {
                OperationResult result = child.RemoveColliderFromAllNodes(collider);
                if (result.Success)
                {
                    return result;
                    // FIXME：需要更新映射表
                }
            }

            // 所有子节点都移除失败，返回移除失败
            return new OperationResult(false);
            // FIXME：移除失败应该不用更新映射表，但还是确认下比较安全
        }
    }
}
