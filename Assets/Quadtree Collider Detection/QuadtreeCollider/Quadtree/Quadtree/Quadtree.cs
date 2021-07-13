using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MtC.Tools.QuadtreeCollider
{
    /// <summary>
    /// 四叉树实例部分
    /// </summary>
    internal partial class Quadtree : MonoBehaviour
    {
        /// <summary>
        /// 四叉树根节点
        /// </summary>
        private QuadtreeNode root = null;
        /// <summary>
        /// 碰撞器到节点的映射表
        /// </summary>
        private readonly Dictionary<QuadtreeCollider, QuadtreeNode> collidersToNodes = new Dictionary<QuadtreeCollider, QuadtreeNode>();

        private void Awake()
        {
            // 节点创建过程中使用了Resources.Load，这个方法不能通过类的字段声明时赋值来调用
            root = new QuadtreeNode(QuadtreeConfig.StartArea);
        }

        private void Update()
        {
            // 更新四叉树
            UpdateQuadtree();

            // 进行检测
            Detect();
        }

        /// <summary>
        /// 更新四叉树
        /// </summary>
        private void UpdateQuadtree()
        {
            // 从根节点开始更新四叉树
            root.Update();
        }

        /// <summary>
        /// 进行碰撞检测
        /// </summary>
        private void Detect()
        {
            // 筛选出所有碰撞器中是检测器的
            List<QuadtreeCollider> detectorsTemp = collidersToNodes.Keys.Where(collider => collider.IsDetector).ToList();

            foreach (QuadtreeCollider detector in detectorsTemp)
            {
                // 获取所有与当前遍历到的碰撞器发生碰撞的碰撞器
                List<QuadtreeCollider> collisionColliders = Instance.root.GetCollidersInCollision(detector);

                // 移除当前遍历的碰撞器本身
                collisionColliders.Remove(detector);

                // 发出碰撞事件
                detector.SendCollision(collisionColliders);
                // XXX：如果在检测时报出空异常等异常，可能是这里没有进行空异常的判断导致的
            }
        }
    }
}
