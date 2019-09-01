using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider
{
    /// <summary>
    /// 四叉树包装类
    /// </summary>
    internal class Quadtree : MonoBehaviour
    {
        /// <summary>
        /// 初始根节点范围
        /// </summary>
        public static Rect _startArea = new Rect(-1, -1, 1922, 1082); // TODO：需要引入配置

        /// <summary>
        /// 实例
        /// </summary>
        private static Quadtree instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                lock (typeof(Quadtree))
                {
                    if (_instance == null)
                    {
                        _instance = new GameObject("Quadtree").AddComponent<Quadtree>();
                        DontDestroyOnLoad(_instance);
                    }
                    return _instance;
                }
            }
        }
        private static Quadtree _instance;

        /// <summary>
        /// 所有检测器
        /// </summary>
        private List<QuadtreeCollider> _detectors = new List<QuadtreeCollider>();
        /// <summary>
        /// 四叉树根节点
        /// </summary>
        private QuadtreeNode _root = new QuadtreeNode(_startArea);

        /// <summary>
        /// 向四叉树中添加碰撞器
        /// </summary>
        /// <param name="collider"></param>
        public static void AddCollider(QuadtreeCollider collider)
        {
            instance._root.AddCollider(collider);

            if (collider.isDetector)
                AddDetector(collider);
        }

        /// <summary>
        /// 添加检测器，只会添加进检测列表，不会添加碰撞器
        /// </summary>
        /// <param name="detector"></param>
        internal static void AddDetector(QuadtreeCollider detector)
        {
            if (!instance._detectors.Contains(detector))
                instance._detectors.Add(detector);
        }

        /// <summary>
        /// 从四叉树中移除碰撞器
        /// </summary>
        /// <param name="collider"></param>
        public static void RemoveCollider(QuadtreeCollider collider)
        {
            //Debug.Log("移除碰撞器：" + collider);
            if (_instance == null)
                return;

            _instance._root.RemoveCollider(collider);

            if (collider.isDetector)
                RemoveDetector(collider);
        }

        /// <summary>
        /// 移除检测器，只会移除出监测列表，不会移除碰撞器
        /// </summary>
        /// <param name="detector"></param>
        internal static void RemoveDetector(QuadtreeCollider detector)
        {
            if (_instance == null)
                return;

            _instance._detectors.Remove(detector);
        }

        private void Update()
        {
            UpdateQuadtree();

            //TODO：检测碰撞
        }

        private void UpdateQuadtree()
        {
            //Debug.Log("更新四叉树");

            _root.Update();
        }
    }
}
