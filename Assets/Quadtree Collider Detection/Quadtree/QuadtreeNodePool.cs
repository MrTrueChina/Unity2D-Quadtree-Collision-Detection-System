using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider
{
    /// <summary>
    /// 四叉树节点对象池
    /// </summary>
    internal static class QuadtreeNodePool
    {
        /// <summary>
        /// 池中最多储存的节点数量
        /// </summary>
        private static int _maxNodesNumber = 100;

        private static Stack<QuadtreeNode> _pool = new Stack<QuadtreeNode>();

        internal static void Put(QuadtreeNode node)
        {
            if (_pool.Count < _maxNodesNumber)
                _pool.Push(node);
        }

        /// <summary>
        /// 获取根节点
        /// </summary>
        /// <param name="area"></param>
        internal static QuadtreeNode Get(Rect area)
        {
            if (_pool.Count > 0)
                return GetNodeFromPool(area);

            return new QuadtreeNode(area);
        }

        private static QuadtreeNode GetNodeFromPool(Rect area)
        {
            QuadtreeNode node = _pool.Pop();

            node.Setup(area);

            return node;
        }

        /// <summary>
        /// 获取普通节点
        /// </summary>
        /// <param name="area"></param>
        /// <param name="parent"></param>
        internal static QuadtreeNode Get(Rect area, QuadtreeNode parent)
        {
            if (_pool.Count > 0)
                return GetNodeFromPool(area, parent);

            return new QuadtreeNode(area, parent);
        }

        private static QuadtreeNode GetNodeFromPool(Rect area, QuadtreeNode parent)
        {
            QuadtreeNode node = _pool.Pop();

            node.Setup(area, parent);

            return node;
        }

        /// <summary>
        /// 获取用于逆向生长的新根节点
        /// </summary>
        /// <param name="area"></param>
        /// <param name="children"></param>
        internal static QuadtreeNode Get(Rect area, List<QuadtreeNode> children, int mainNodeIndex)
        {
            if (_pool.Count > 0)
                return GetNodeFromPool(area, children, mainNodeIndex);

            return new QuadtreeNode(area, children, mainNodeIndex);
        }

        private static QuadtreeNode GetNodeFromPool(Rect area, List<QuadtreeNode> children, int mainNodeIndex)
        {
            QuadtreeNode node = _pool.Pop();

            node.Setup(area, children, mainNodeIndex);

            return node;
        }
    }
}
