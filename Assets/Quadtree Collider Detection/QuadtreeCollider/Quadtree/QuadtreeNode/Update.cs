using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider
{
    internal partial class QuadtreeNode
    {
        /// <summary>
        /// 更新碰撞器位置
        /// </summary>
        /// <returns></returns>
        internal OperationResult UpdatePosition()
        {
            if (HaveChildren())
            {
                // 有子节点，通知子节点更新碰撞器位置
                return UpdateChildrenPosition();
            }
            else
            {
                // 没有子节点，更新当前节点内的碰撞器位置
                return UpdateSelfPosition();
            }
        }

        /// <summary>
        /// 更新子节点的碰撞器位置
        /// </summary>
        private OperationResult UpdateChildrenPosition()
        {
            // 更新碰撞器位置必定成功
            OperationResult result = new OperationResult(true);

            // 通知所有子节点更新碰撞器位置
            foreach (QuadtreeNode child in children)
            {
                // 通知子节点更新碰撞器位置
                OperationResult childResult = child.UpdatePosition();

                // 将子节更新导致的映射表更新合并到总结果里
                result.CollidersToNodes.OverlayMerge(childResult.CollidersToNodes);
            }

            // 如果达到了合并标准则进行合并
            if (NeedMerge())
            {
                // 合并所有子节点
                OperationResult mergeResult = Merge();

                // 记录映射表变化
                result.CollidersToNodes.OverlayMerge(mergeResult.CollidersToNodes);
            }

            // 这里需要在所有子节点更新完毕后进行是否需要合并的判断，在遍历子节点的过程中决不能合并节点，否则可能遍历到一半就到了合并标准，之后的遍历就会发生逻辑错误

            return result;
        }

        /// <summary>
        /// 更新当前节点内的碰撞器的位置
        /// </summary>
        private OperationResult UpdateSelfPosition()
        {
            // 更新碰撞器位置必定成功
            OperationResult result = new OperationResult(true);

            // 移除所有当前节点保存的、已经离开当前节点范围的碰撞器，并将这些碰撞器保存起来
            OperationResult removeResult = GetAndRemoveCollidersOutOfArea();

            // 合并移除导致的映射表变更
            result.CollidersToNodes.OverlayMerge(removeResult.CollidersToNodes);

            // 把移除的碰撞器重新存入四叉树
            OperationResult resetResult = ResetCollidersIntoQuadtree(new List<QuadtreeCollider>(removeResult.CollidersToNodes.Keys));

            // 合并重新存入导致的映射表变更
            result.CollidersToNodes.OverlayMerge(resetResult.CollidersToNodes);

            return result;
        }

        /// <summary>
        /// 移除所有当前节点保存的、已经离开当前节点范围的碰撞器，并返回
        /// </summary>
        /// <returns></returns>
        private OperationResult GetAndRemoveCollidersOutOfArea()
        {
            OperationResult result = new OperationResult(true);

            // 遍历所有碰撞器，超出节点范围的记录下来
            foreach (QuadtreeCollider collider in colliders)
            {
                if (!area.Contains(collider.Position))
                {
                    result.CollidersToNodes.Add(collider, null);
                }
            }

            // 将所有超出节点范围的碰撞器移除出四叉树
            foreach (QuadtreeCollider collider in new List<QuadtreeCollider>(result.CollidersToNodes.Keys))
            {
                // 移除碰撞器
                OperationResult removeResult = Quadtree.RemoveCollider(collider);

                // 记录映射表的变化
                result.CollidersToNodes.OverlayMerge(removeResult.CollidersToNodes);
            }

            // 返回结果
            return result;
        }

        /// <summary>
        /// 将碰撞器重新存入四叉树
        /// </summary>
        /// <param name="outOfFieldColliders"></param>
        private OperationResult ResetCollidersIntoQuadtree(List<QuadtreeCollider> outOfFieldColliders)
        {
            // 重新存入必定成功
            OperationResult result = new OperationResult(true);

            // 通过包装类将碰撞器从根节点存入
            foreach (QuadtreeCollider collider in outOfFieldColliders)
            {
                // 通过包装类重新存入碰撞器
                OperationResult addResult = Quadtree.AddCollider(collider);

                // 将映射表变更合并进结果中
                result.CollidersToNodes.OverlayMerge(addResult.CollidersToNodes);
            }

            // 这里并不需要担心重新存入导致分割问题，首先重新存入的是越界的，不会存入当前节点，当前节点不会分割。
            // 如果存到已更新的节点，已更新的节点分割不影响操作。如果存到未更新的节点，为更新的节点分割只会在遍历到的时候把更新下发到分割出的子节点中，不会导致逻辑问题。

            return result;
        }

        /// <summary>
        /// 更新最大半径
        /// </summary>
        /// <returns></returns>
        internal float UpdateMaxRadius()
        {
            if (HaveChildren())
            {
                // 有子节点，通知子节点更新最大半径，并在子节点更新后设置当前节点的最大半径
                return maxRadius = UpdateChildrenMaxRadius();
            }
            else
            {
                // 没有子节点，更新当前节点的最大半径
                return maxRadius = UpdateSelfMaxRadius();
            }
        }

        /// <summary>
        /// 更新子节点的最大半径
        /// </summary>
        /// <returns></returns>
        private float UpdateChildrenMaxRadius()
        {
            maxRadius = DEFAULT_MAX_RADIUS;

            // 遍历所有子节点更新最大半径，并保留最大的半径作为当前节点的最大半径
            foreach (QuadtreeNode child in children)
            {
                maxRadius = Mathf.Max(maxRadius, child.UpdateMaxRadius());
            }

            // 返回最大半径
            return maxRadius;
        }

        /// <summary>
        /// 更新当前节点的最大半径
        /// </summary>
        /// <returns></returns>
        private float UpdateSelfMaxRadius()
        {
            maxRadius = DEFAULT_MAX_RADIUS;

            // 遍历所有碰撞器，找出最大半径
            foreach (QuadtreeCollider collider in colliders)
            {
                if (collider.MaxRadius > maxRadius)
                {
                    maxRadius = collider.MaxRadius;
                }
            }

            // 返回最大半径
            return maxRadius;
        }
    }
}
