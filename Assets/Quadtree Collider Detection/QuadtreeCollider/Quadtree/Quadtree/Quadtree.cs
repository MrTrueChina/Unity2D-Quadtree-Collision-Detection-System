using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider
{
    /// <summary>
    /// 四叉树实例部分
    /// </summary>
    internal partial class Quadtree : MonoBehaviour
    {
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
            // 节点创建过程中使用了Resources.Load，这个方法不能通过类的字段声明时赋值来调用
            _root = new QuadtreeNode(QuadtreeConfig.startArea);
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
            _root.Update();
        }

        /// <summary>
        /// 进行碰撞检测
        /// </summary>
        private void Detect()
        {
            // 防止在进行检测时发生检测器列表的变化，直接用碰撞器列表内容创建新列表
            List<QuadtreeCollider> detectors = new List<QuadtreeCollider>(_detectors);

            foreach (QuadtreeCollider detector in detectors)
            {
                // 获取所有与当前遍历到的碰撞器发生碰撞的碰撞器
                List<QuadtreeCollider> collisionColliders = instance._root.GetCollidersInCollision(detector);

                // 移除当前遍历的碰撞器本身
                collisionColliders.Remove(detector);

                // 发出碰撞事件
                detector.SendCollision(collisionColliders);
                // XXX：如果在检测时报出空异常等异常，可能是这里没有进行空异常的判断导致的
            }
        }
    }
}
