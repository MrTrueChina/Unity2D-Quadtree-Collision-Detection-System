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
        internal bool AddColliderByArea(QuadtreeCollider collider)
        {
            // 碰撞器不在节点范围内，返回存入失败
            if (!_area.Contains(collider.Position))
            {
                return false;
            }

            // 有子节点，发给子节点保存
            if (HaveChildren())
            {
                return AddColliderIntoChildrenByArea(collider);
            }

            // 在范围内，而且没有子节点，保存节点并返回保存成功
            AddColliderIntoSelf(collider);
            return true;
        }

        /// <summary>
        /// 通过节点范围向子节点存入碰撞器，只当碰撞器在节点范围内时才存入
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        private bool AddColliderIntoChildrenByArea(QuadtreeCollider collider)
        {
            return AddColliderIntoChildren(collider, (node, colliderTemp) => node.AddColliderByArea(colliderTemp));
        }

        /// <summary>
        /// 根据碰撞器相对于节点的位置向四叉树中存入碰撞器
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        private bool AddColliderByDirection(QuadtreeCollider collider)
        {
            // 当前节点相对于父节点的方向与碰撞器相对于父节点的方向，在 X 轴上是否一致
            bool colliderAndNodeOnSameXSide = !((area.center.x > _parent.area.center.x) ^ (collider.Position.x > _parent.area.center.x));
            // 当前节点相对于父节点的方向与碰撞器相对于父节点的方向，在 Y 轴上是否一致
            bool colliderAndNodeOnSameYSide = !((area.center.y > _parent.area.center.y) ^ (collider.Position.y > _parent.area.center.y));

            // 只要有一个不一致的，就说明碰撞器不应该被存入到这个节点里，直接返回
            if (!colliderAndNodeOnSameXSide || !colliderAndNodeOnSameYSide)
            {
                return false;
            }

            // 有子节点，发给子节点保存
            if (HaveChildren())
            {
                return AddColliderIntoChildrenByDirection(collider);
            }

            // 没有子节点，保存节点并返回保存成功
            AddColliderIntoSelf(collider);
            return true;
        }

        /// <summary>
        /// 根据碰撞器相对于节点的方向向子节点存入碰撞器，只当碰撞器相对父节点的方向和节点相对父节点的方向相同时才存入
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        private bool AddColliderIntoChildrenByDirection(QuadtreeCollider collider)
        {
            return AddColliderIntoChildren(collider, (node, colliderTemp) => node.AddColliderByDirection(colliderTemp));
        }

        /// <summary>
        /// 将碰撞器按照指定标准存入到子节点
        /// </summary>
        /// <param name="collider">要存入的碰撞器</param>
        /// <param name="addCollider">存入标准，返回 true 表示碰撞器可以存入到这个子节点中</param>
        /// <returns></returns>
        private bool AddColliderIntoChildren(QuadtreeCollider collider, Func<QuadtreeNode, QuadtreeCollider,bool> addCollider)
        {
            // 遍历子节点存入碰撞器
            foreach (QuadtreeNode child in _children)
            {
                // 如果有一个子节点存入成功则返回存入成功
                if (addCollider(child, collider))
                {
                    return true;
                }
            }

            // 正常流程中不会运行到的所有子节点都保存失败的情况
            throw new ArgumentOutOfRangeException("向范围是 " + _area + " 的节点的子节点存入碰撞器 " + collider + " 时发生错误：碰撞器没有存入任何子节点");
        }

        /// <summary>
        /// 向当前节点添加碰撞器
        /// </summary>
        /// <param name="collider"></param>
        private void AddColliderIntoSelf(QuadtreeCollider collider)
        {
            // 添加进碰撞器列表
            _colliders.Add(collider);

            // 如果需要分割节点则进行分割
            if (NeedSplit())
            {
                Split();
            }
        }

        /// <summary>
        /// 检测是否需要分割节点
        /// </summary>
        /// <returns></returns>
        private bool NeedSplit()
        {
            return 
                // 碰撞器数量超过节点内最大碰撞器数量
                _colliders.Count > QuadtreeConfig.maxCollidersNumber
                // 节点高度超过节点最小高度
                && _area.height > QuadtreeConfig.minSideLength
                // 节点宽度超过节点最小宽度
                && _area.width > QuadtreeConfig.minSideLength;
        }

        /// <summary>
        /// 分割节点
        /// </summary>
        private void Split()
        {
            // 创建子节点
            CreateChildren();

            // 把碰撞器分发给子节点
            SetAllColliderIntoChindren();
        }

        /// <summary>
        /// 创建子节点
        /// </summary>
        private void CreateChildren()
        {
            // 计算出宽高的一半用于创建子节点，先算出一半是为了防止可能出现的计算误差
            float halfWidth = _area.width / 2;
            float halfHeight = _area.height / 2;

            // 创建子节点
            _children = new List<QuadtreeNode>
            {
                new QuadtreeNode(new Rect(_area.x + halfWidth, _area.y + halfHeight, _area.width - halfWidth, _area.height - halfHeight), this), // 右上子节点
                new QuadtreeNode(new Rect(_area.x + halfWidth, _area.y, _area.width - halfWidth, halfHeight), this), // 右下子节点
                new QuadtreeNode(new Rect(_area.x, _area.y, halfWidth, halfHeight), this), // 左下子节点
                new QuadtreeNode(new Rect(_area.x, _area.y + halfHeight, halfWidth, _area.height - halfHeight), this) // 左上子节点
            };
        }

        /// <summary>
        /// 把碰撞器分发给子节点
        /// </summary>
        private void SetAllColliderIntoChindren()
        {
            // 把当前节点的碰撞器全部存入到子节点，这里为了防止可能有碰撞器已经离开了节点范围，需要根据方向而不是范围存入
            foreach (QuadtreeCollider collider in _colliders)
            {
                AddColliderIntoChildrenByDirection(collider);
            }

            // 清空当前节点存储的碰撞器
            _colliders.Clear();

            // 此处如果使用先移除越界的碰撞器分割后重新存入树，则有可能因为移除节点导致需要合并，形成 分割反而导致了合并 的逻辑套娃
        }
    }
}
