using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MtC.Tools.QuadtreeCollider
{
    // 从树中移除碰撞器的部分
    internal partial class QuadtreeNode
    {
        /// <summary>
        /// 从当前节点中移除指定碰撞器，不进行合并
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        internal OperationResult RemoveColliderFromSelfWithOutMerge(QuadtreeCollider collider)
        {
            bool listResult = colliders.Remove(collider);

            if (listResult)
            {
                // 创建操作成功的返回对象
                OperationResult result = new OperationResult(true);

                // 映射表值为 null 表示碰撞器不属于任何节点
                result.CollidersToNodes.Add(collider, null);

                return result;
            }
            else
            {
                // 返回移除失败，移除失败不改变映射表，直接返回失败即可
                return new OperationResult(false);
            }
        }

        /// <summary>
        /// 从当前节点中移除指定碰撞器，并根据需要合并
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        internal OperationResult RemoveColliderFromSelfWithMerge(QuadtreeCollider collider)
        {
            // 移除碰撞器
            OperationResult result = RemoveColliderFromSelfWithOutMerge(collider);

            // 移除失败，不需要合并，直接返回结果
            if(!result.Success)
            {
                return result;
            }

            // 向上合并
            OperationResult mergeResult = UpwordMerge();

            // 如果合并成功，将合并导致的映射表更新加入到结果中
            if (mergeResult.Success)
            {
                result.CollidersToNodes.OverlayMerge(mergeResult.CollidersToNodes);
            }

            return result;
        }

        /// <summary>
        /// 如果当前节点达到了合并条件，返回 true
        /// </summary>
        /// <returns></returns>
        private bool NeedMerge()
        {
            // 有子节点，且节点中的碰撞器总数小于最小碰撞器总数，就是达到了合并条件
            return
                HaveChildren()
                && GetColliderNumbers() < QuadtreeConfig.MinCollidersNumber;
        }

        /// <summary>
        /// 获取当前节点及所有子节点的总计碰撞器数量
        /// </summary>
        /// <returns></returns>
        private int GetColliderNumbers()
        {
            if (HaveChildren())
            {
                // 有子节点，子节点的碰撞器数量和就是这个节点的碰撞器数量
                int number = 0;
                children.ForEach(child => number += child.GetColliderNumbers());
                return number;
            }
            else
            {
                // 没有子节点，返回碰撞器列表数量
                return colliders.Count;
            }
        }

        /// <summary>
        /// 从当前节点开始，向上合并节点，直到不能合并为止
        /// </summary>
        /// <returns></returns>
        private OperationResult UpwordMerge()
        {
            // 先准备一个合并失败的结果
            OperationResult result = new OperationResult(false);
            
            // 从当前节点开始
            QuadtreeNode currentNode = this;

            // 逻辑比较繁琐，使用死循环加跳出
            while (true)
            {
                // 当前节点是 null，这种情况是合并完了根节点后的循环，直接结束循环
                if (currentNode == null)
                {
                    break;
                }

                // 当前节点是末梢，末梢本身不能合并，向上一级
                if (!currentNode.HaveChildren())
                {
                    // 向上一级
                    currentNode = currentNode.parent;

                    // 再次循环
                    continue;
                }

                // 当前节点不能合并，结束合并
                if (!currentNode.NeedMerge())
                {
                    break;
                }

                // 合并并记录结果
                result = currentNode.Merge();

                // 向上移一级
                currentNode = currentNode.parent;
            }

            return result;
        }

        /// <summary>
        /// 合并当前节点
        /// </summary>
        /// <returns></returns>
        private OperationResult Merge()
        {
            // 合并节点必然成功
            OperationResult result = new OperationResult(true);

            // 将当前节点和所有子节点的碰撞器移除并记录
            result.CollidersToNodes.OverlayMerge(RemoveAllColliders().CollidersToNodes);

            // 把子节点列表抛弃掉
            children = null;

            // 把移除的子节点重新添加到当前节点中，这里使用直接添加，因为合并操作并不会导致节点达到分割标准
            OperationResult addCollidersResult = AddCollidersIntoSelf(result.CollidersToNodes.Select(pair => pair.Key).ToList());

            // 更新映射表修改记录
            result.CollidersToNodes.OverlayMerge(addCollidersResult.CollidersToNodes);

            // 返回记录
            return result;
        }

        /// <summary>
        /// 将当前节点和所有子节点的碰撞器移除
        /// </summary>
        /// <returns></returns>
        private OperationResult RemoveAllColliders()
        {
            // 清空碰撞器必然成功
            OperationResult result = new OperationResult(true);

            if (!HaveChildren())
            {
                // 没有子节点，移除碰撞器并返回

                // 将所有碰撞器记录到操作结果里，映射的节点是 null，表示从树里移除
                result.CollidersToNodes = colliders.Select(collider => new KeyValuePair<QuadtreeCollider, QuadtreeNode>(collider, null)).ToDictionary(pair => pair.Key, pair => pair.Value);

                // 清空碰撞器列表
                colliders.Clear();

                return result;
            }
            else
            {
                // 有子节点，通知子节点清空碰撞器并合并记录

                children.ForEach(child =>
                {
                    result.CollidersToNodes.OverlayMerge(child.RemoveAllColliders().CollidersToNodes);
                });

                return result;
            }
        }

        /// <summary>
        /// 向当前节点批量添加碰撞器
        /// </summary>
        /// <param name="newColliders"></param>
        /// <returns></returns>
        private OperationResult AddCollidersIntoSelf(List<QuadtreeCollider> newColliders)
        {
            // 批量添加必然成功
            OperationResult result = new OperationResult(true);

            // 将碰撞器添加到碰撞器列表里
            colliders.AddRange(newColliders);

            // 去重
            colliders = new List<QuadtreeCollider>(colliders.Distinct());

            // 记录映射变化
            result.CollidersToNodes = newColliders
                .Where(collider => collider != null)
                .Select(collider => new KeyValuePair<QuadtreeCollider, QuadtreeNode>(collider, this))
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            return result;
        }
    }
}
