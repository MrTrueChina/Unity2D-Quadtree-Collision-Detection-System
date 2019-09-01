using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider
{
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

        public override float maxRadius => radius;

        protected override void DrawColliderGizomoSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, _radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x),Mathf.Abs(transform.lossyScale.y))); //TODO：后期可以考虑通过配置文件达到不同的面向方向
        }

        private void OnValidate()
        {
            if (_radius < 0)
                _radius = 0;
        }
    }
}
