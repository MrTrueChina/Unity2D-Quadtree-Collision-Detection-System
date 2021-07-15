using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MtC.Tools.QuadtreeCollider
{
    /// <summary>
    /// 绘制碰撞器的组件
    /// </summary>
    public class CollisionDrawer : MonoBehaviour, IOnQuadtreeCollisionStay, IOnQuadtreeCollisionEnter, IOnQuadtreeCollisionExit
    {
        private readonly List<QuadtreeCollider> colliders = new List<QuadtreeCollider>();

        public void OnQuadtreeCollisionEnter(QuadtreeCollider collider)
        {
            Debug.Log("碰撞器 " + collider.GetInstanceID() + " 进入碰撞器 " + GetInstanceID() + " 的范围");
        }

        public void OnQuadtreeCollisionStay(QuadtreeCollider collider)
        {
            colliders.Add(collider);
        }

        public void OnQuadtreeCollisionExit(QuadtreeCollider collider)
        {
            Debug.Log("碰撞器 " + collider.GetInstanceID() + " 离开碰撞器 " + GetInstanceID() + " 的范围");
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
