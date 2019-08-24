using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
            List<QuadtreeCollider> removeColliders = GetRemoveColliders(isSetColliders);
            List<QuadtreeCollider> addColliders = GetAddColliders(isSetColliders);

            addColliders.AddRange(RemoveColliders(removeColliders));
            AddColliders(addColliders);
            UpdateCollidersNumber();
            UpdateSplitAndMerge();
            UpdateMaxRadius();
        }

        private List<QuadtreeCollider> GetRemoveColliders(Dictionary<QuadtreeCollider, bool> isSetColliders)
        {
            List<QuadtreeCollider> removeColliders = new List<QuadtreeCollider>();

            foreach (KeyValuePair<QuadtreeCollider, bool> pair in isSetColliders)
                if (!pair.Value)
                    removeColliders.Add(pair.Key);

            return removeColliders;
        }

        private List<QuadtreeCollider> GetAddColliders(Dictionary<QuadtreeCollider, bool> isSetColliders)
        {
            List<QuadtreeCollider> addColliders = new List<QuadtreeCollider>();

            foreach (KeyValuePair<QuadtreeCollider, bool> pair in isSetColliders)
                if (pair.Value)
                    addColliders.Add(pair.Key);

            return addColliders;
        }

        private List<QuadtreeCollider> RemoveColliders(List<QuadtreeCollider> removeColliders)
        {
            if (haveChild())
                return RemoveCollidersFormChildren(removeColliders);

            return RemoveCollidersFromSelf(removeColliders);
        }

        private List<QuadtreeCollider> RemoveCollidersFormChildren(List<QuadtreeCollider> removeColliders)
        {
            List<QuadtreeCollider> ReSetColliders = new List<QuadtreeCollider>();

            foreach (QuadtreeNode child in _children)
                ReSetColliders.AddRange(child.RemoveColliders(removeColliders));

            return ReSetColliders;
        }

        private List<QuadtreeCollider> RemoveCollidersFromSelf(List<QuadtreeCollider> removeColliders)
        {
            List<QuadtreeCollider> ReSetColliders = new List<QuadtreeCollider>();

            IEnumerator enumerator = removeColliders.GetEnumerator();
            while (enumerator.MoveNext())
            {
                QuadtreeCollider collider = (QuadtreeCollider)enumerator.Current;
                if (removeColliders.Contains(enumerator.Current))
                {
                    removeColliders.Remove(collider);
                    _colliders.Remove(collider);
                }
                else if (!_area.Contains(collider.position)) //TODO: 移除脱离了节点区域的碰撞器，这部分需要重构
                {
                    if (_colliders.Remove(collider)) // 如果能移除则说明需要重新存入
                        removeColliders.Add(collider);
                }
            }

            return ReSetColliders;
        }

        private void AddColliders(List<QuadtreeCollider> addColliders)
        {
            if (haveChild())
                AddCollidersIntoChildren(addColliders);
            else
                AddCollidersIntoSelf(addColliders);
        }

        private void AddCollidersIntoChildren(List<QuadtreeCollider> addColliders)
        {
            //TODO:向子节点添加碰撞器
            throw new NotImplementedException();
        }

        private void AddCollidersIntoSelf(List<QuadtreeCollider> addColliders)
        {
            //TODO:向自己添加碰撞器
            throw new NotImplementedException();
        }

        /// <summary>
        /// 向四叉树中添加碰撞器
        /// </summary>
        /// <param name="collider"> 添加进四叉树中的碰撞器 </param>
        /// <returns> 如果成功添加碰撞器则返回true </returns>
        //public bool AddCollider(QuadtreeCollider collider)
        //{
        //    if (haveChild())
        //        return AddColliderIntoChildren(collider);

        //    return AddColliderIntoSelf(collider);
        //}

        private bool haveChild()
        {
            return _children != null; // TODO：如果有存入池的部分，这个判断可能需要根据入池时子节点清空还是改null做改变
        }

        //private bool AddColliderIntoChildren(QuadtreeCollider collider)
        //{
        //    foreach (QuadtreeNode child in _children)
        //        if (child.AddCollider(collider))
        //            return true;
        //    return false;
        //}

        //private bool AddColliderIntoSelf(QuadtreeCollider collider)
        //{
        //    _colliders.Add(collider);

        //    return true;
        //}

        //private void UpdateCollidersAndArea(Dictionary<QuadtreeCollider, bool> isSetColliders)
        //{
        //    if (haveChild())
        //        UpdateChildrenCollidersAndArea(isSetColliders);

        //    UpdateSelfCollidersAndArea(isSetColliders);
        //}

        //private void UpdateChildrenCollidersAndArea(Dictionary<QuadtreeCollider, bool> isSetColliders)
        //{
        //    foreach (QuadtreeNode child in _children)
        //        child.UpdateCollidersAndArea(isSetColliders);
        //}

        //private void UpdateSelfCollidersAndArea(Dictionary<QuadtreeCollider, bool> isSetColliders)
        //{
        //    RemoveCollidersFromSelf(isSetColliders);
        //    UpdateSelfCollidersArea();
        //    AddCollidersIntoSelf(isSetColliders);
        //}

        private void UpdateSelfCollidersArea()
        {
            //TODO:更新自己的碰撞器所属的区域
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
