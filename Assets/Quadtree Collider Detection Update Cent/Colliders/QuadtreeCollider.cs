using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider.UpdateCent
{
    /// <summary>
    /// 四叉树碰撞检测的碰撞器
    /// </summary>
    public abstract class QuadtreeCollider : MonoBehaviour
    {
        /// <summary>
        /// 默认的未选择状态下的碰撞器Gizmo颜色
        /// </summary>
        protected static readonly Color DEFAULT_GIZMO_COLOR = Color.green;
        /// <summary>
        /// 默认的选择状态下的碰撞器Gizmo颜色
        /// </summary>
        protected static readonly Color DEFAULT_GIZMO_COLOR_SELECTED = Color.yellow;

        /// <summary>
        /// 最大半径，在这个半径外的碰撞器不会发生碰撞
        /// </summary>
        public abstract float maxRadius { get; }
        /// <summary>
        /// 碰撞器在世界空间内的位置
        /// </summary>
        public Vector3 position
        {
            get { return _transform.position; } //TODO：需要一些中间步骤以应对四叉树平面不是竖着的的情况
        }

        protected Transform _transform;

        private void Awake()
        {
            _transform = transform;
        }

        private void OnEnable()
        {
            Quadtree.AddCollider(this);
        }

        private void OnDisable()
        {
            Quadtree.RemoveCollider(this);
        }

        private void OnDrawGizmos()
        {
            DrawCollider();
        }

        /// <summary>
        /// 当物体没有被选择时按照这个方法绘制碰撞器
        /// </summary>
        protected abstract void DrawCollider();

        private void OnDrawGizmosSelected()
        {
            DrawColliderSelected();
        }

        /// <summary>
        /// 当物体被选择时按照这个方法绘制碰撞器
        /// </summary>
        protected abstract void DrawColliderSelected();
    }
}
