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
        /// 在没有碰撞器时的默认最大检测半径
        /// </summary>
        private const float DEFAULT_MAX_RADIUS = Mathf.NegativeInfinity;

        /// <summary>
        /// 需要分割的碰撞器数量，节点内碰撞器数量超过这个值后即认为需要分割
        /// </summary>
        private static int _splitCollidersNumber = 10;
        /// <summary>
        /// 需要合并的碰撞器数量，节点内碰撞器数量小于这个值后即认为需要合并
        /// </summary>
        private static int _mergeCollidersNumber = 5;
        /// <summary>
        /// 节点区域最小边长，节点任意一边长小于此值则不能继续分割
        /// </summary>
        private static int _minSide = 10; // 这个值用于应对碰撞器位置过近导致的过度分割情况

        /// <summary>
        /// 节点范围
        /// </summary>
        private Rect _area = default;
        /// <summary>
        /// 父节点
        /// </summary>
        private QuadtreeNode _parent = null;
        /// <summary>
        /// 子节点列表
        /// </summary>
        private List<QuadtreeNode> _children = null;
        /// <summary>
        /// 节点中所有碰撞器的最大检测半径
        /// </summary>
        private float _maxRadius = DEFAULT_MAX_RADIUS; // 默认最大检测半径是负无穷，这样无论碰撞器半径和位置都会判断这个节点不会发生碰撞
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
            if (HaveChild())
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

            IEnumerator<QuadtreeCollider> enumerator = removeColliders.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (removeColliders.Contains(enumerator.Current))
                {
                    removeColliders.Remove(enumerator.Current);
                    _colliders.Remove(enumerator.Current);
                }
                else if (!_area.Contains(enumerator.Current.position)) //TODO: 移除脱离了节点区域的碰撞器，这部分需要重构
                {
                    if (_colliders.Remove(enumerator.Current)) // 如果能移除则说明需要重新存入
                        removeColliders.Add(enumerator.Current);
                }
            }

            return ReSetColliders;
        }

        private void AddColliders(List<QuadtreeCollider> addColliders)
        {
            //Debug.Log("向区域为 " + _area + " 的节点添加碰撞器 " + addColliders + "，元素数 " + addColliders.Count + " 个");

            if (HaveChild())
                AddCollidersIntoChildren(addColliders);
            else
                AddCollidersIntoSelf(addColliders);
        }

        private void AddCollidersIntoChildren(List<QuadtreeCollider> addColliders)
        {
            foreach (QuadtreeNode child in _children)
                child.AddColliders(addColliders);
        }

        private void AddCollidersIntoSelf(List<QuadtreeCollider> addColliders)
        {
            for (int i = addColliders.Count - 1; i > -1; i--)
                if (_area.Contains(addColliders[i].position))
                {
                    _colliders.Add(addColliders[i]);
                    addColliders.RemoveAt(i);
                }
        }

        private bool HaveChild()
        {
            //Debug.Log("判断节点 " + this + " 是否有子节点：" + (_children != null));
            return _children != null; // TODO：如果有存入池的部分，这个判断可能需要根据入池时子节点清空还是改null做改变
        }

        private int UpdateCollidersNumber()
        {
            if (HaveChild())
                return _collidersNumber = UpdateChildrenCollidersNumber();
            else
                return _collidersNumber = _colliders.Count();
        }

        private int UpdateChildrenCollidersNumber()
        {
            int collidersNumber = 0;

            foreach (QuadtreeNode child in _children)
                collidersNumber += child.UpdateCollidersNumber();

            return collidersNumber;
        }

        private void UpdateSplitAndMerge()
        {
            if (HaveChild())
                UpdateSplitAndMergeHaveChildren();
            else
                UpdateSelfSplit(); // 没有子节点的节点没有合并的可能性，只更新分割
        }

        private void UpdateSplitAndMergeHaveChildren()
        {
            if (NeetMerge())
                Merge();
            else
                UpdateChildrenSplitAndMerge();
        }

        private bool NeetMerge()
        {
            return _collidersNumber < _mergeCollidersNumber;
        }

        private void Merge()
        {
            _colliders = GetChildrensColliders();
            ClearChildren();
        }

        private List<QuadtreeCollider> GetChildrensColliders()
        {
            List<QuadtreeCollider> colliders = new List<QuadtreeCollider>();

            foreach (QuadtreeNode child in _children)
                colliders.AddRange(child.GetColliders());

            return colliders;

        }

        private IEnumerable<QuadtreeCollider> GetColliders()
        {
            if (HaveChild())
                return GetChildrensColliders();
            else
                return GetSelfColliders();
        }

        private IEnumerable<QuadtreeCollider> GetSelfColliders()
        {
            return _colliders;
        }

        private void ClearChildren()
        {
            foreach (QuadtreeNode child in _children)
                Clear();
        }

        private void Clear()
        {
            if (HaveChild())
                ClearChildren();

            ClearSelf(); // 无论是否清理子节点，都要清理自身
        }

        private void ClearSelf()
        {
            _colliders.Clear();
            _children.Clear();

            _area = default;
            _parent = null;
            _children = null;
            _maxRadius = DEFAULT_MAX_RADIUS;
            _collidersNumber = 0;
        }

        private void UpdateChildrenSplitAndMerge()
        {
            foreach (QuadtreeNode child in _children)
                child.UpdateSplitAndMerge();
        }

        private void UpdateSelfSplit()
        {
            if (NeedSplit())
                Split();
        }

        private bool NeedSplit()
        {
            return _collidersNumber > _splitCollidersNumber && _area.width > _minSide && _area.height > _minSide;
        }

        private void Split()
        {
            SplitSelf();
            UpdateChildrenSplit();
        }

        private void SplitSelf()
        {
            CreateChildren();
            AddCollidersIntoChildren(_colliders);

            _colliders.Clear();
        }

        private void CreateChildren()
        {
            float halfWidth = _area.width / 2;
            float halfHeight = _area.height / 2;

            _children = new List<QuadtreeNode>();

            _children.Add(new QuadtreeNode(new Rect(_area.x + halfWidth, _area.y + halfHeight, _area.width - halfWidth, _area.height - halfHeight), this));
            _children.Add(new QuadtreeNode(new Rect(_area.x + halfWidth, _area.y, _area.width - halfWidth, halfHeight), this));
            _children.Add(new QuadtreeNode(new Rect(_area.x, _area.y, halfWidth, halfHeight), this));
            _children.Add(new QuadtreeNode(new Rect(_area.x, _area.y + halfHeight, halfWidth, _area.height - halfHeight), this));
        }

        private void UpdateChildrenSplit()
        {
            foreach (QuadtreeNode child in _children)
                child.UpdateSelfSplit();
        }

        private float UpdateMaxRadius()
        {
            if (HaveChild())
                return _maxRadius = UpdateChildrenMaxRadius();
            return GetMaxRadiusFromSelf();
        }

        private float UpdateChildrenMaxRadius()
        {
            float maxRadiusInChildren = DEFAULT_MAX_RADIUS;

            foreach (QuadtreeNode child in _children)
                maxRadiusInChildren = Mathf.Max(maxRadiusInChildren, child.UpdateMaxRadius()); // 同时进行向子节点递归和保存最大半径

            return maxRadiusInChildren;
        }

        private float GetMaxRadiusFromSelf()
        {
            float maxRadius = DEFAULT_MAX_RADIUS;

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
