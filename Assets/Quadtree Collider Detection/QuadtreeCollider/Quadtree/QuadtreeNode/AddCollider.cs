using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider
{
    // 将碰撞器加入到四叉树中的部分
    internal partial class QuadtreeNode
    {
        /// <summary>
        /// 根据节点范围向四叉树中存入碰撞器，只当碰撞器在节点范围内时才存入
        /// </summary>
        /// <param name="collider">存入的碰撞器</param>
        /// <returns> 如果成功存入，返回 true </returns>
        internal OperationResult AddColliderByArea(QuadtreeCollider collider)
        {
            return AddCollider(collider, (nodeParam, colliderParam) =>
            {
                // 如果碰撞器在节点范围内，说明碰撞器可以存入这个节点
                return nodeParam.Area.Contains(colliderParam.Position);
            });
        }

        /// <summary>
        /// 根据指定标准存入碰撞器
        /// </summary>
        /// <param name="collider">要存入的碰撞器</param>
        /// <param name="canAdd">如果这个方法返回 true 这说明这个碰撞器可以存入当前节点</param>
        /// <returns></returns>
        private OperationResult AddCollider(QuadtreeCollider collider, Func<QuadtreeNode, QuadtreeCollider, bool> canAdd)
        {
            // 不符合存入标准的直接返回存入失败
            if (!canAdd(this, collider))
            {
                return new OperationResult(false);
                // FIXME：这里应该不需要操作节点表，但还是确认一下比较安全
            }

            // 有子节点，发给子节点保存
            if (HaveChildren())
            {
                return AddColliderIntoChildren(collider, (nodeParam, colliderParam) => nodeParam.AddCollider(colliderParam, canAdd));
            }

            // 没有子节点，保存节点并返回结果
            return AddColliderIntoSelf(collider);
        }

        /// <summary>
        /// 将碰撞器按照指定标准存入到子节点
        /// </summary>
        /// <param name="collider">要存入的碰撞器</param>
        /// <param name="addCollider">存入方法</param>
        /// <returns></returns>
        private OperationResult AddColliderIntoChildren(QuadtreeCollider collider, Func<QuadtreeNode, QuadtreeCollider,OperationResult> addCollider)
        {
            // 遍历子节点存入碰撞器
            foreach (QuadtreeNode child in children)
            {
                // 如果有一个子节点存入成功，则将这个节点的操作结果作为结果返回
                OperationResult result = addCollider(child, collider);
                if (result.Success)
                {
                    return result;
                }
            }

            // 正常流程中不会运行到的所有子节点都保存失败的情况
            throw new ArgumentOutOfRangeException("向范围是 " + area + " 的节点的子节点存入碰撞器 " + collider + " 时发生错误：碰撞器没有存入任何子节点");
        }

        /// <summary>
        /// 向当前节点添加碰撞器
        /// </summary>
        /// <param name="collider"></param>
        private OperationResult AddColliderIntoSelf(QuadtreeCollider collider)
        {
            // 向当前节点添加肯定是成功的
            OperationResult result = new OperationResult(true);

            // 添加进碰撞器列表
            colliders.Add(collider);
            // FIXME：需要记录映射表

            // 如果需要分割节点则进行分割
            if (NeedSplit())
            {
                Split();
                // FIXME：分割节点需要更新映射表
            }

            return result;
        }

        /// <summary>
        /// 检测是否需要分割节点
        /// </summary>
        /// <returns></returns>
        private bool NeedSplit()
        {
            return 
                // 碰撞器数量超过节点内最大碰撞器数量
                colliders.Count > QuadtreeConfig.MaxCollidersNumber
                // 节点高度超过节点最小高度
                && area.height > QuadtreeConfig.MinSideLength
                // 节点宽度超过节点最小宽度
                && area.width > QuadtreeConfig.MinSideLength;
        }

        /// <summary>
        /// 分割节点
        /// </summary>
        private OperationResult Split()
        {
            // 创建子节点
            CreateChildren();

            // 把碰撞器分发给子节点，返回操作结果
            return SetAllColliderIntoChindren();
        }

        /// <summary>
        /// 创建子节点
        /// </summary>
        private void CreateChildren()
        {
            // 计算出宽高的一半用于创建子节点，先算出一半是为了防止可能出现的计算误差
            float halfWidth = area.width / 2;
            float halfHeight = area.height / 2;

            // 创建子节点
            children = new List<QuadtreeNode>
            {
                new QuadtreeNode(new Rect(area.x + halfWidth, area.y + halfHeight, area.width - halfWidth, area.height - halfHeight), this), // 右上子节点
                new QuadtreeNode(new Rect(area.x + halfWidth, area.y, area.width - halfWidth, halfHeight), this), // 右下子节点
                new QuadtreeNode(new Rect(area.x, area.y, halfWidth, halfHeight), this), // 左下子节点
                new QuadtreeNode(new Rect(area.x, area.y + halfHeight, halfWidth, area.height - halfHeight), this) // 左上子节点
            };
        }

        /// <summary>
        /// 把碰撞器分发给子节点
        /// </summary>
        private OperationResult SetAllColliderIntoChindren()
        {
            // 分发操作必然成功
            OperationResult result = new OperationResult(true);

            // 把当前节点的碰撞器全部存入到子节点，这里为了防止可能有碰撞器已经离开了节点范围，需要根据方向而不是范围存入
            foreach (QuadtreeCollider collider in colliders)
            {
                AddColliderIntoChildren(collider, (nodeParam, colliderParam) => nodeParam.AddColliderByDirection(colliderParam));
                // FIXME：这里需要记录操作后的映射表
            }

            // 清空当前节点存储的碰撞器
            colliders.Clear();

            return result;

            // 分发功能如果使用先移除越界的碰撞器分割后重新存入树，则有可能因为移除节点导致需要合并，形成 分割反而导致了合并 的逻辑套娃
        }

        /// <summary>
        /// 根据碰撞器相对于节点的位置向四叉树中存入碰撞器
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        private OperationResult AddColliderByDirection(QuadtreeCollider collider)
        {
            return AddCollider(collider, (nodeParam, colliderParam) =>
            {
                // 当前节点相对于父节点的方向与碰撞器相对于父节点的方向，在 X 轴上是否一致
                bool colliderAndNodeOnSameXSide = !((nodeParam.Area.center.x > nodeParam.parent.Area.center.x) ^ (colliderParam.Position.x > nodeParam.parent.Area.center.x));
                // 当前节点相对于父节点的方向与碰撞器相对于父节点的方向，在 Y 轴上是否一致
                bool colliderAndNodeOnSameYSide = !((nodeParam.Area.center.y > nodeParam.parent.Area.center.y) ^ (colliderParam.Position.y > nodeParam.parent.Area.center.y));

                // 两个方向都一致，说明碰撞器可以存入这个节点
                return colliderAndNodeOnSameXSide && colliderAndNodeOnSameYSide;
            });
        }
    }
}
