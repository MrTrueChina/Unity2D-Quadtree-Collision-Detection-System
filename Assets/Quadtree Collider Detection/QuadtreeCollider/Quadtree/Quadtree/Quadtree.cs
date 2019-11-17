using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider
{
    // 实例部分
    /// <summary>
    /// 四叉树包装类
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
            _root = new QuadtreeNode(QuadtreeConfig.startArea); // 节点创建过程中使用了Resources.Load，这个方法不能通过类的字段声明时赋值来调用
        }

        private void Update()
        {
            UpdateQuadtree();

            Detect();
        }

        private void UpdateQuadtree()
        {
            _root.Update();
        }

        private void Detect()
        {
            List<QuadtreeCollider> detectors = new List<QuadtreeCollider>(_detectors); // 防止在进行检测时发生检测器列表的变化，直接用碰撞器列表内容创建新列表
            foreach (QuadtreeCollider detector in detectors)
            {
                List<QuadtreeCollider> collisionColliders = instance._root.GetCollidersInCollision(detector);
                collisionColliders.Remove(detector);
                detector.SendCollision(collisionColliders); //TODO：如果在检测时报出空异常等异常，可能是这里没有进行空异常的判断导致的
            }
        }
    }
}
