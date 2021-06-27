using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider
{
    /// <summary>
    /// 圆形四叉树碰撞器
    /// </summary>
    public class CircleQuadtreeCollider : QuadtreeCollider
    {
        /// <summary>
        /// 半径
        /// </summary>
        public float radius
        {
            get
            {
                return _radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));
                //TODO：后期可以考虑通过配置文件达到不同的面向方向
            }
            set { _radius = value; }
        }
        [SerializeField]
        private float _radius;

        // 圆形碰撞器的最大半径就是半径
        internal override float maxRadius => radius;

        protected override void DrawColliderGizomoSelected()
        {
            // 检测器是黄色，碰撞器是绿色
            Gizmos.color = (isDetector ? Color.yellow : Color.green) * 0.8f;

            // 绘制圆形
            MyGizmos.DrawCircle(transform.position, radius);
        }

        private void OnValidate()
        {
            // 限制编辑时半径不能小于 0
            if (_radius < 0)
            {
                _radius = 0;
            }
        }
    }
}
