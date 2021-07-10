using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MtC.Tools.QuadtreeCollider
{
    /// <summary>
    /// 绘制碰撞器的组件
    /// </summary>
    public class CollisionDrawer : MonoBehaviour, IOnQuadtreeCollisionStay
    {
        private readonly List<QuadtreeCollider> colliders = new List<QuadtreeCollider>();

        public void OnQuadtreeCollisionStay(QuadtreeCollider collider)
        {
            colliders.Add(collider);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow * 0.8f;

            foreach (QuadtreeCollider collider in colliders)
            {
                if (collider != null)
                {
                    Gizmos.DrawLine(transform.position, collider.transform.position);
                }
            }

            colliders.Clear();
        }
    }
}
