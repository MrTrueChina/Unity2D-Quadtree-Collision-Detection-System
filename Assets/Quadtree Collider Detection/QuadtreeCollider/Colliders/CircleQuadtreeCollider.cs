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
                return _radius * Mathf.Max(Mathf.Abs(_transform.lossyScale.x), Mathf.Abs(_transform.lossyScale.y)); //TODO：后期可以考虑通过配置文件达到不同的面向方向
            }
            set { _radius = value; }
        }
        [SerializeField]
        private float _radius;

        internal override float maxRadius => radius;

        protected override void DrawColliderGizomoSelected()
        {
            Gizmos.color = (isDetector ? Color.yellow : Color.green) * 0.8f;

            Vector3 beginPoint = transform.position + Vector3.right * _radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));       //三角函数角度是从正右方开始的，画圆起始点是最右边的点raw
            Gizmos.DrawLine(transform.position, beginPoint);
            for (int i = 1; i <= 144; i++)
            {
                float angle = 2 * Mathf.PI / 144 * i;

                float x = _radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y)) * Mathf.Cos(angle) + transform.position.x;
                float y = _radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y)) * Mathf.Sin(angle) + transform.position.y;
                Vector3 endPoint = new Vector3(x, y, transform.position.z);

                Gizmos.DrawLine(beginPoint, endPoint);

                beginPoint = endPoint;
            }
        }

        private void OnValidate()
        {
            if (_radius < 0)
                _radius = 0;
        }
    }
}
