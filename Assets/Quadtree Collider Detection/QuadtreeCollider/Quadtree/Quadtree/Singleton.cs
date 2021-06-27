﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider
{
    /// <summary>
    /// 四叉树单例部分
    /// </summary>
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
                        // 创建一个带四叉树组件的对象，并设为不随场景加载销毁
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
            // 向实例中添加碰撞器
            instance.DoAddCollider(collider);

            // 如果是检测器则添加检测器
            if (collider.isDetector)
            {
                AddDetector(collider);
            }
        }

        /// <summary>
        /// 在重新存入碰撞器时使用的存入方法，不会改变检测器列表
        /// </summary>
        /// <param name="collider"></param>
        internal static void AddColliderOnReset(QuadtreeCollider collider)
        {
            // 向实例中添加碰撞器
            instance.DoAddCollider(collider);

            // 重新存入碰撞器是将四叉树中存在的碰撞器取出来重新存入，前后的碰撞器列表并没有变化，检测器列表更不会变化，省一步快一步
        }

        /// <summary>
        /// 添加检测器，只会添加进检测列表，不会添加碰撞器
        /// </summary>
        /// <param name="detector"></param>
        internal static void AddDetector(QuadtreeCollider detector)
        {
            // 实例的检测器列表里没有这个碰撞器则添加进去
            if (!instance._detectors.Contains(detector))
            {
                instance._detectors.Add(detector);
            }
        }

        /// <summary>
        /// 从四叉树中移除碰撞器
        /// </summary>
        /// <param name="collider"></param>
        public static void RemoveCollider(QuadtreeCollider collider)
        {
            // 没有实例则不进行操作
            if (_instance == null)
            {
                return;
            }

            // 从根节点开始移除碰撞器
            _instance._root.RemoveCollider(collider);

            // 如果要移除的碰撞器是检测器，移除检测器
            if (collider.isDetector)
            {
                RemoveDetector(collider);
            }
        }

        /// <summary>
        /// 移除检测器，只会移除出监测列表，不会移除碰撞器
        /// </summary>
        /// <param name="detector"></param>
        internal static void RemoveDetector(QuadtreeCollider detector)
        {
            // 没有实例则不进行操作
            if (_instance == null)
            {
                return;
            }

            // 从实例的碰撞器列表里移除这个碰撞器
            _instance._detectors.Remove(detector);
        }
    }
}