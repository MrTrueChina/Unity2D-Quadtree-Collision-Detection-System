using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider
{
    /// <summary>
    /// 四叉树节点
    /// </summary>
    internal partial class QuadtreeNode
    {
        /// <summary>
        /// 右上子节点的索引
        /// </summary>
        private const int RIGHT_TOP_CHILD_INDEX = 0;
        /// <summary>
        /// 右下子节点的索引
        /// </summary>
        private const int RIGHT_BOTTOM_CHILD_INDEX = 1;
        /// <summary>
        /// 左下子节点的索引
        /// </summary>
        private const int LEFT_BOTTOM_CHILD_INDEX = 2;
        /// <summary>
        /// 左上子节点的索引
        /// </summary>
        private const int LEFT_TOP_CHILD_INDEX = 3;
        /// <summary>
        /// 默认最大检测半径
        /// </summary>
        private const float DEFAULT_MAX_RADIUS = Mathf.NegativeInfinity; // 默认最大检测半径为负无穷，这样无论检测器半径多大都不会判断为可能发生碰撞，减少无意义的向子节点递归

        /// <summary>
        /// 父节点
        /// </summary>
        private QuadtreeNode _parent = null;
        /// <summary>
        /// 四叉树节点所拥有的区域
        /// </summary>
        private Rect _area = default;
        /// <summary>
        /// 这个节点所拥有的碰撞器
        /// </summary>
        private List<QuadtreeCollider> _colliders = new List<QuadtreeCollider>();
        /// <summary>
        /// 这个节点所拥有的的子节点
        /// </summary>
        private List<QuadtreeNode> _children = null;
        /// <summary>
        /// 这个节点所拥有的所有碰撞器中，需要检测半径最长的碰撞器的检测半径
        /// </summary>
        private float _maxRadius = DEFAULT_MAX_RADIUS;

        /// <summary>
        /// 根节点的构造方法，只有区域没有父节点。根节点
        /// </summary>
        /// <param name="area"></param>
        internal QuadtreeNode(Rect area)
        {
            _area = area;
        }

        /// <summary>
        /// 根节点之外的节点的构造方法
        /// </summary>
        /// <param name="area"></param>
        /// <param name="parent"></param>
        internal QuadtreeNode(Rect area, QuadtreeNode parent) : this(area)
        {
            _parent = parent;
        }

        /// <summary>
        /// 直接存入子节点的构造方法，用于逆向生长
        /// </summary>
        /// <param name="area"></param>
        /// <param name="children"></param>
        internal QuadtreeNode(Rect area, List<QuadtreeNode> children, int mainNodeIndex) : this(area)
        {
            _children = children;

            _maxRadius = children[mainNodeIndex]._maxRadius;
        }

        private bool HaveChildren()
        {
            return _children != null; // 子节点List只在创建子节点时才会创建，判断是不是null就能判断有没有子节点
        }
    }
}
