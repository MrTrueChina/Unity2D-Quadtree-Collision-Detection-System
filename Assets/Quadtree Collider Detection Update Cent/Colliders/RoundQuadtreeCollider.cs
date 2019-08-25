using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MtC.Tools.GizmosTools;

namespace MtC.Tools.QuadtreeCollider.UpdateCent
{
    public class RoundQuadtreeCollider : QuadtreeCollider
    {
#pragma warning disable 0649
        [SerializeField]
        private float _radius;

        public override float maxRadius
        {
            get
            {
                return _radius * Mathf.Max(_transform.lossyScale.x, _transform.lossyScale.y, _transform.lossyScale.z); //TODO：这里有问题，需要添加对方向的考虑
            }
        }

        protected override void DrawCollider()
        {
            Gizmos.color = DEFAULT_GIZMO_COLOR;

            GizmosTool.DrawCircle(transform.position, transform.rotation, _radius);
        }

        protected override void DrawColliderSelected()
        {
            Gizmos.color = DEFAULT_GIZMO_COLOR_SELECTED;

            GizmosTool.DrawCircle(transform.position, transform.rotation, _radius);
        }
    }
}
