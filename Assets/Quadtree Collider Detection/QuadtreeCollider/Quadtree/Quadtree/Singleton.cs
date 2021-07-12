using System.Collections;
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
        private static Quadtree Instance
        {
            get
            {
                if (instance != null)
                    return instance;

                lock (typeof(Quadtree))
                {
                    if (instance == null)
                    {
                        // 创建一个带四叉树组件的对象，并设为不随场景加载销毁
                        instance = new GameObject("Quadtree").AddComponent<Quadtree>();
                        DontDestroyOnLoad(instance);
                    }
                    return instance;
                }
            }
        }
        private static Quadtree instance;

        /// <summary>
        /// 向四叉树中添加碰撞器
        /// </summary>
        /// <param name="collider"></param>
        public static void AddCollider(QuadtreeCollider collider)
        {
            // FIXME：这里需要加一个不能重复存入的处理，使用映射表就可以处理

            // 向实例中添加碰撞器
            Instance.DoAddCollider(collider);

            // 如果是检测器则添加检测器
            if (collider.IsDetector)
            {
                AddDetector(collider);
            }
        }

        // FIXME：这个不走标准流程的方法需要删掉
        /// <summary>
        /// 在重新存入碰撞器时使用的存入方法，不会改变检测器列表
        /// </summary>
        /// <param name="collider"></param>
        internal static void AddColliderOnReset(QuadtreeCollider collider)
        {
            // 向实例中添加碰撞器
            Instance.DoAddCollider(collider);

            // 重新存入碰撞器是将四叉树中存在的碰撞器取出来重新存入，前后的碰撞器列表并没有变化，检测器列表更不会变化，省一步快一步
        }

        /// <summary>
        /// 添加检测器，只会添加进检测列表，不会添加碰撞器
        /// </summary>
        /// <param name="detector"></param>
        internal static void AddDetector(QuadtreeCollider detector)
        {
            // 实例的检测器列表里没有这个碰撞器则添加进去
            if (!Instance.detectors.Contains(detector))
            {
                Instance.detectors.Add(detector);
            }
        }

        /// <summary>
        /// 从四叉树中移除碰撞器
        /// </summary>
        /// <param name="collider"></param>
        public static void RemoveCollider(QuadtreeCollider collider)
        {
            // FIXME：在有了映射表之后可以通过映射表对不存在的碰撞器移除进行拦截

            // 没有实例则不进行操作
            if (instance == null)
            {
                return;
            }

            // 从根节点开始移除碰撞器
            QuadtreeNode.OperationResult result = instance.root.RemoveCollider(collider);

            // 移除成功进行后续操作
            if (result.Success)
            {
                // 覆盖合并映射表并移除空值
                Instance.collidersToNodes.OverlayMerge(result.CollidersToNodes).RemoveOnValueIsNull();

                // 如果要移除的碰撞器是检测器，移除检测器
                if (collider.IsDetector)
                {
                    RemoveDetector(collider);
                }
            }
        }

        /// <summary>
        /// 移除检测器，只会移除出监测列表，不会移除碰撞器
        /// </summary>
        /// <param name="detector"></param>
        internal static void RemoveDetector(QuadtreeCollider detector)
        {
            // 没有实例则不进行操作
            if (instance == null)
            {
                return;
            }

            // 从实例的碰撞器列表里移除这个碰撞器
            instance.detectors.Remove(detector);
        }
    }
}