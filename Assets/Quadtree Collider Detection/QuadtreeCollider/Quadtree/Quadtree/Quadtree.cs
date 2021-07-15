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
            // 从根节点开始更新碰撞器位置
            QuadtreeNode.OperationResult positionResult = root.UpdatePosition();

            // 更新映射表
            collidersToNodes.OverlayMerge(positionResult.CollidersToNodes).RemoveOnValueIsNull();

            // 从根节点更新最大半径
            root.UpdateMaxRadius();

            // 从包装类两次调用而不是从根节点一次调用的原因是，更新位置时可能有的节点跑到了树外面，这就需要生长四叉树，根节点就会改变
            // 因为生长出的节点原来是没有碰撞器的，因此这些节点不需要进行碰撞器位置越界更新
            // 但这些节点需要进行最大半径更新，否则新节点的最大半径是负无穷，会导致生长的这一帧碰撞检测错误
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
            }
        }
    }
}
