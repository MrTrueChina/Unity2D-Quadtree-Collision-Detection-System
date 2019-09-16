﻿using System;
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
            if (!_area.Contains(collider.position))
                return false;

            if (HaveChildren())
                return AddColliderIntoChildren(collider);

            AddColliderIntoSelf(collider);
            return true;
        }

        private bool AddColliderIntoChildren(QuadtreeCollider collider)
        {
            foreach (QuadtreeNode child in _children)
                if (child.AddCollider(collider))
                    return true;

            throw new ArgumentOutOfRangeException("向范围是 " + _area + " 的节点的子节点存入碰撞器 " + collider + " 时发生错误：碰撞器没有存入任何子节点"); // 正常流程中不会运行到这
        }

        private void AddColliderIntoSelf(QuadtreeCollider collider)
        {
            _colliders.Add(collider);

            if (NeedSplit())
                Split();
        }

        private bool NeedSplit()
        {
            return _colliders.Count > QuadtreeConfig.maxCollidersNumber && _area.height > QuadtreeConfig.minSideLength && _area.width > QuadtreeConfig.minSideLength;
        }

        private void Split()
        {
            /*
             *  清除掉不在自己区域内的碰撞器，防止下发碰撞器失败
             *  分割处子节点并下发碰撞器
             *  把清除掉的那些碰撞器重新存入四叉树
             *  
             *  实际是进行了一次位置更新，但为了防止节点碰撞器互相越界导致的多重更新将分割写在存入和取出中间
             */
            List<QuadtreeCollider> outOfAreaColliders = GetAndRemoveCollidersOutOfField();
            DoSplite();
            ResetCollidersIntoQuadtree(outOfAreaColliders);
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
    }
}
