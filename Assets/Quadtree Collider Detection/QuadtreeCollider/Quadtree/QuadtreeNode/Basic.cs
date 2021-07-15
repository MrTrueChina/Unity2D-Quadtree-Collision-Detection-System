using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
        internal const int RIGHT_TOP_CHILD_INDEX = 0;
        /// <summary>
        /// 右下子节点的索引
        /// </summary>
        internal const int RIGHT_BOTTOM_CHILD_INDEX = 1;
        /// <summary>
        /// 左下子节点的索引
        /// </summary>
        internal const int LEFT_BOTTOM_CHILD_INDEX = 2;
        /// <summary>
        /// 左上子节点的索引
        /// </summary>
        internal const int LEFT_TOP_CHILD_INDEX = 3;
        /// <summary>
        /// 默认最大检测半径
        /// </summary>
        private const float DEFAULT_MAX_RADIUS = Mathf.NegativeInfinity; // 默认最大检测半径为负无穷，这样无论检测器半径多大都不会判断为可能发生碰撞，减少无意义的向子节点递归

        /// <summary>
        /// 父节点
        /// </summary>
        private QuadtreeNode parent = null;
        /// <summary>
        /// 四叉树节点所拥有的区域
        /// </summary>
        internal Rect Area
        {
            get { return area; }     
        }
        private Rect area = default;
        /// <summary>
        /// 这个节点所拥有的碰撞器
        /// </summary>
        private List<QuadtreeCollider> colliders = new List<QuadtreeCollider>();
        /// <summary>
        /// 这个节点所拥有的的子节点
        /// </summary>
        private List<QuadtreeNode> children = null;
        /// <summary>
        /// 这个节点所拥有的所有碰撞器中，需要检测半径最长的碰撞器的检测半径
        /// </summary>
        private float maxRadius = DEFAULT_MAX_RADIUS;

        /// <summary>
        /// 根节点的构造方法，只有区域没有父节点。根节点
        /// </summary>
        /// <param name="area"></param>
        internal QuadtreeNode(Rect area)
        {
            this.area = area;
        }

        /// <summary>
        /// 根节点之外的节点的构造方法
        /// </summary>
        /// <param name="area"></param>
        /// <param name="parent"></param>
        internal QuadtreeNode(Rect area, QuadtreeNode parent) : this(area)
        {
            this.parent = parent;
        }

        /// <summary>
        /// 直接存入子节点的构造方法，用于逆向生长
        /// </summary>
        /// <param name="area"></param>
        /// <param name="children"></param>
        internal QuadtreeNode(Rect area, List<QuadtreeNode> children, int mainNodeIndex) : this(area)
        {
            this.children = children;

            // 逆向生长出的节点没有碰撞器，之前根节点的最大半径就是新的根节点的最大半径
            maxRadius = children[mainNodeIndex].maxRadius;

            // 给所有子节点设置父节点
            foreach (QuadtreeNode child in this.children)
            {
                child.parent = this;
            }
        }

        /// <summary>
        /// 检测当前节点是否有子节点
        /// </summary>
        /// <returns></returns>
        private bool HaveChildren()
        {
            // 子节点List只在创建子节点时才会创建，判断是不是null就能判断有没有子节点
            return children != null;
        }
    }

    /// <summary>
    /// <see cref="Dictionary{TKey, TValue}"/> 的扩展方法类
    /// </summary>
    internal static partial class DictionaryExtension
    {
        /// <summary>
        /// 将指定的 Dictionary 中的内容直接覆盖进调用这个方法的 Dictionary 中<br/>
        /// 【注意】这个方法会导致调用的 Dictionary 内容变化
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="mainDictonary"></param>
        /// <param name="subDictonary"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> OverlayMerge<TKey, TValue>(this Dictionary<TKey, TValue> mainDictonary, Dictionary<TKey, TValue> subDictonary)
        {
            // 遍历整个 subDictionary
            foreach(KeyValuePair<TKey,TValue> pair in subDictonary)
            {
                // 使用根据索引存值的特性，如果没有这个 Key 则添加，有这个 Key 则覆盖
                mainDictonary[pair.Key] = subDictonary[pair.Key];
            }

            return mainDictonary;
        }

        /// <summary>
        /// 移除掉 Value 为 null 的值<br/>
        /// 【注意】这个方法会导致调用的 Dictionary 内容变化
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="mainDictonary"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> RemoveOnValueIsNull<TKey, TValue>(this Dictionary<TKey, TValue> mainDictonary)
        {
            List<TKey> nullKeys = mainDictonary.Where(pair => pair.Value == null).Select(pair => pair.Key).ToList();

            nullKeys.ForEach(key =>
            {
                mainDictonary.Remove(key);
            });

            return mainDictonary;
        }
    }
}
