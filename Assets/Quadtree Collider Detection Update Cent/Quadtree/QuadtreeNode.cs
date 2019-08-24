using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider.UpdateCent
{
    /// <summary>
    /// 四叉树节点
    /// </summary>
    public class QuadtreeNode
    {
        /// <summary>
        /// 需要分割的碰撞器数量，节点内碰撞器数量超过这个值后即认为需要分割
        /// </summary>
        private static int _splitCollidersNumber = 10;
        /// <summary>
        /// 需要合并的碰撞器数量，节点内碰撞器数量小于这个值后即认为需要合并
        /// </summary>
        private static int _mergeColliderNumber = 5;

        /// <summary>
        /// 节点范围
        /// </summary>
        private Rect _area;
        /// <summary>
        /// 父节点
        /// </summary>
        private QuadtreeNode _parent = null;
        /// <summary>
        /// 子节点列表
        /// </summary>
        private List<QuadtreeNode> _children = new List<QuadtreeNode>();
        /// <summary>
        /// 节点中所有碰撞器的最大检测半径
        /// </summary>
        private float _maxRadius = Mathf.NegativeInfinity; // 默认最大检测半径是负无穷，这样无论碰撞器半径和位置都会判断这个节点不会发生碰撞
        /// <summary>
        /// 节点中的碰撞器数量
        /// </summary>
        private int _collidersNumber = 0;
        /// <summary>
        /// 节点中的碰撞器
        /// </summary>
        private List<QuadtreeCollider> _colliders = new List<QuadtreeCollider>();

        /// <summary>
        /// 创建根节点的构造方法
        /// </summary>
        /// <param name="area"> 节点范围 </param>
        public QuadtreeNode(Rect area)
        {
            _area = area;
        }

        /// <summary>
        /// 创建普通节点的构造方法
        /// </summary>
        /// <param name="area"> 节点范围 </param>
        /// <param name="parent"> 父节点 </param>
        public QuadtreeNode(Rect area, QuadtreeNode parent) : this(area)
        {
            _parent = parent;
        }

        /// <summary>
        /// 创建有指定子节点的根节点的构造方法
        /// </summary>
        /// <param name="area"> 节点范围 </param>
        /// <param name="children"> 子节点 </param>
        public QuadtreeNode(Rect area, List<QuadtreeNode> children) : this(area)
        {
            _children = children;
        }

        public void Update(Dictionary<QuadtreeCollider, bool> isSetColliders)
        {
            UpdateCollidersAndArea();
            UpdateCollidersNumber();
            UpdateSplitAndMerge();
            UpdateMaxRadius();
        }

        /// <summary>
        /// 向四叉树中添加碰撞器
        /// </summary>
        /// <param name="collider"> 添加进四叉树中的碰撞器 </param>
        /// <returns> 如果成功添加碰撞器则返回true </returns>
        public bool AddCollider(QuadtreeCollider collider)
        {
            if (haveChild())
                return AddColliderIntoChildren(collider);

            return AddColliderIntoSelf(collider);
        }

        private bool haveChild()
        {
            return _children != null; // TODO：如果有存入池的部分，这个判断可能需要根据入池时子节点清空还是改null做改变
        }

        private bool AddColliderIntoChildren(QuadtreeCollider collider)
        {
            //TODO：向子节点添加碰撞器
            throw new NotImplementedException();
        }

        private bool AddColliderIntoSelf(QuadtreeCollider collider)
        {
            //TODO：向自己添加碰撞器
            throw new NotImplementedException();
        }

        private void UpdateCollidersAndArea()
        {
            //TODO：存入/取出碰撞器并更新碰撞器所属节点
            throw new NotImplementedException();
        }

        private void UpdateCollidersNumber()
        {
            //TODO：更新碰撞器数量
            throw new NotImplementedException();
        }

        private void UpdateSplitAndMerge()
        {
            //TODO：更新分割与合并
            throw new NotImplementedException();
        }

        private float UpdateMaxRadius()
        {
            if (haveChild())
                return _maxRadius = UpdateChildrenMaxRadius();
            return GetMaxRadiusFromSelf();
        }

        private float UpdateChildrenMaxRadius()
        {
            float maxRadiusInChildren = Mathf.NegativeInfinity;

            foreach (QuadtreeNode child in _children)
                maxRadiusInChildren = Mathf.Max(maxRadiusInChildren, child.UpdateMaxRadius()); // 同时进行向子节点递归和保存最大半径

            return maxRadiusInChildren;
        }

        private float GetMaxRadiusFromSelf()
        {
            float maxRadius = Mathf.NegativeInfinity;

            foreach (QuadtreeCollider collider in _colliders)
                maxRadius = maxRadius > collider.maxRadius ? maxRadius : collider.maxRadius;

            return maxRadius;
        }

        public List<QuadtreeCollider> CheckCollition(QuadtreeCollider collider)
        {
            //TODO：碰撞检测
            throw new Exception();
        }
    }
}
