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
        private List<QuadtreeCollider> _colliders = new List<QuadtreeCollider>();

        private void Start()
        {
            GetComponent<QuadtreeCollider>().SubscribeCollisionStay(OnQuadtreeCollisionStay);
            GetComponent<QuadtreeCollider>().SubscribeCollisionStay(OnQuadtreeCollisionStay);
            GetComponent<QuadtreeCollider>().SubscribeCollisionStay(OnQuadtreeCollisionStay);
            GetComponent<QuadtreeCollider>().SubscribeCollisionStay(OnQuadtreeCollisionStay);
        }

        public void OnQuadtreeCollisionStay(QuadtreeCollider collider)
        {
            Debug.Log(GetInstanceID());

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
