using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider
{
    /// <summary>
    /// 绘制碰撞器的组件
    /// </summary>
    public class CollisionDrawer : MonoBehaviour, IOnQuadtreeCollisionStay
    {
        private List<QuadtreeCollider> _colliders = new List<QuadtreeCollider>();

        public void OnQuadtreeCollisionStay(QuadtreeCollider collider)
        {
            _colliders.Add(collider);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow * 0.8f;

            foreach (QuadtreeCollider collider in _colliders)
            {
                if (collider != null)
                {
                    Gizmos.DrawLine(transform.position, collider.transform.position);
                }
            }

            _colliders.Clear();
        }
    }
}
