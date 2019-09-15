using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider
{
    // 单例部分
    internal partial class Quadtree : MonoBehaviour
    {
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
        /// 向四叉树中添加碰撞器
        /// </summary>
        /// <param name="collider"></param>
        public static void AddCollider(QuadtreeCollider collider)
        {
            instance.DoAddCollider(collider);

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
    }
}