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
        /// 向四叉树中存入碰撞器
        /// </summary>
        /// <param name="collider">存入的碰撞器</param>
        /// <returns> 如果成功存入，返回 true </returns>
        internal bool AddCollider(QuadtreeCollider collider)
        {
            // 碰撞器不在节点范围内，返回存入失败
            if (!_area.Contains(collider.position))
            {
                return false;
            }

            // 有子节点，发给子节点保存
            if (HaveChildren())
            {
                return AddColliderIntoChildren(collider);
            }

            // 保存节点并返回保存成功
            AddColliderIntoSelf(collider);
            return true;
        }

        /// <summary>
        /// 向子节点存入碰撞器
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        private bool AddColliderIntoChildren(QuadtreeCollider collider)
        {
            // 遍历子节点存入碰撞器
            foreach (QuadtreeNode child in _children)
            {
                // 如果有一个子节点存入成功则返回存入成功
                if (child.AddCollider(collider))
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
            /*
             *  清除掉不在自己区域内的碰撞器，防止下发碰撞器失败
             *  分割处子节点并下发碰撞器
             *  把清除掉的那些碰撞器重新存入四叉树
             *  
             *  实际是进行了一次位置更新，但为了防止节点碰撞器互相越界导致的多重更新将分割写在存入和取出中间
             */
            // 将节点保存的但是已经离开了节点范围的碰撞器从四叉树中移除并保存
            List<QuadtreeCollider> outOfAreaColliders = GetAndRemoveCollidersOutOfField();

            // 进行分割
            DoSplite();

            // 将之前移除的节点重新存入四叉树
            ResetCollidersIntoQuadtree(outOfAreaColliders);
        }

        /// <summary>
        /// 进行分割节点
        /// </summary>
        private void DoSplite()
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
            // 把当前节点的碰撞器全部存入到子节点
            foreach (QuadtreeCollider collider in _colliders)
            {
                AddColliderIntoChildren(collider);
            }

            // 清空当前节点存储的碰撞器
            _colliders.Clear();
        }
    }
}
