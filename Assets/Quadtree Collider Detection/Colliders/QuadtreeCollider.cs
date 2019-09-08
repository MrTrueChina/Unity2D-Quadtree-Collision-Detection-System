using System;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider
{
    /// <summary>
    /// 四叉树碰撞检测的碰撞器
    /// </summary>
    public abstract class QuadtreeCollider : MonoBehaviour
    {
        protected Transform _transform;

        private List<QuadtreeCollider> _lastCollisionColliders = new List<QuadtreeCollider>();

        private Action<QuadtreeCollider> _collisionEnterEventHandler;
        private Action<QuadtreeCollider> _collisionStayEventHandler;
        private Action<QuadtreeCollider> _collisionExitEventHandler;

        public bool autoSubscribe { get { return _autoSubscribe; } }
        [SerializeField]
        private bool _autoSubscribe = true;

        /// <summary>
        /// 碰撞器的位置
        /// </summary>
        public Vector2 position
        {
            get { return _transform.position; }
        }

        /// <summary>
        /// 碰撞器需要检测的最大半径，超过这个半径则认为不会发生碰撞
        /// </summary>
        public abstract float maxRadius
        {
            get;
        }

        /// <summary>
        /// 这个碰撞器是不是碰撞检测器
        /// </summary>
        public bool isDetector
        {
            get
            {
                return _isDetector;
            }
            set
            {
                if (_isDetector != value) // 只有有变化时才处理，更新碰撞器成本可以省下来
                {
                    _isDetector = value;

                    if (_isDetector)
                        Quadtree.AddDetector(this);
                    else
                        Quadtree.RemoveDetector(this);
                }
            }
        }
        [SerializeField]
        private bool _isDetector;

        private void Awake()
        {
            _transform = transform;

            if (_autoSubscribe)
                foreach (Component component in GetComponents<Component>())
                {
                    if (component is IOnQuadtreeCollisionEnter)
                        _collisionEnterEventHandler += (component as IOnQuadtreeCollisionEnter).OnQuadtreeCollisionEnter;
                    if (component is IOnQuadtreeCollisionStay)
                        _collisionStayEventHandler += (component as IOnQuadtreeCollisionStay).OnQuadtreeCollisionStay;
                    if (component is IOnQuadtreeCollisionExit)
                        _collisionExitEventHandler += (component as IOnQuadtreeCollisionExit).OnQuadtreeCollisionExit;
                }
        }

        private void OnEnable()
        {
            Quadtree.AddCollider(this);
        }

        private void OnDisable()
        {
            Quadtree.RemoveCollider(this);
        }

        internal void SendCollision(List<QuadtreeCollider> collisionColliders)
        {
            foreach (QuadtreeCollider collider in collisionColliders)
            {
                if (!_lastCollisionColliders.Contains(collider))
                    _collisionEnterEventHandler?.Invoke(collider);

                _collisionStayEventHandler?.Invoke(collider);
            }

            foreach (QuadtreeCollider collider in _lastCollisionColliders)
                if (!collisionColliders.Contains(collider))
                    _collisionExitEventHandler?.Invoke(collider);

            _lastCollisionColliders = collisionColliders;
        }

        public bool IsCollitionToCollider(QuadtreeCollider collider)
        {
            return QuadtreeCollisionDetector.IsCollition(this, collider);
        }

        private void OnDrawGizmosSelected()
        {
            DrawColliderGizomoSelected();
        }

        /// <summary>
        /// 在选中时绘制碰撞器
        /// </summary>
        protected abstract void DrawColliderGizomoSelected();
    }
}
