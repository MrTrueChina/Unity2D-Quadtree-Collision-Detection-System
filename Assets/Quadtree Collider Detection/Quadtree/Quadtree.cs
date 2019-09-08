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
        private QuadtreeNode _root = null;

        private void Awake()
        {
            _root = new QuadtreeNode(QuadtreeConfig.startArea); // 节点创建过程中使用了Resources.Load，这个方法不能通过类的字段声明时赋值来调用
        }

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

            Detect();
        }

        private void UpdateQuadtree()
        {
            //Debug.Log("更新四叉树");

            _root.Update();
        }

        private void Detect()
        {
            List<QuadtreeCollider> detectors = new List<QuadtreeCollider>(_detectors);
            Debug.Log("发起碰撞检测，检测器数量 = " + detectors.Count);
            foreach (QuadtreeCollider detector in detectors)
            {
                List<QuadtreeCollider> collisionColliders = instance._root.GetCollidersInCollision(detector);
                collisionColliders.Remove(detector); // TODO：结果有误，只有两个碰撞器的时候应该只有一个碰撞到的碰撞器，实际结果是两个，这一步的移除可能没有成功
                detector.SendCollision(collisionColliders); //TODO：如果在检测时报出空异常等异常，可能是这里没有进行空异常的判断导致的
            }
            Debug.Log("第一个碰撞器检测到的碰撞数量有 " + instance._root.GetCollidersInCollision(_detectors[0]).Count + " 个");
        }
    }
}
