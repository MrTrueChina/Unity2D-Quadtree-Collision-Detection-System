﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MtC.Tools.QuadtreeCollider
{
    /// <summary>
    /// 四叉树碰撞检测的碰撞器
    /// </summary>
    public abstract class QuadtreeCollider : MonoBehaviour
    {
        /// <summary>
        /// 上一次碰撞检测时碰撞到的碰撞器
        /// </summary>
        private List<QuadtreeCollider> lastCollisionColliders = new List<QuadtreeCollider>();

        private UnityEvent<QuadtreeCollider> collisionEnterEventHandler = new UnityEvent<QuadtreeCollider>();
        private UnityEvent<QuadtreeCollider> collisionStayEventHandler = new UnityEvent<QuadtreeCollider>();
        private UnityEvent<QuadtreeCollider> collisionExitEventHandler = new UnityEvent<QuadtreeCollider>();

        /// <summary>
        /// 是否自动订阅
        /// </summary>
        public bool AutoSubscribe { get { return autoSubscribe; } }
        [SerializeField]
        [Header("自动订阅")]
        private bool autoSubscribe = true;

        /// <summary>
        /// 碰撞器的位置
        /// </summary>
        internal Vector2 Position
        {
            get { return transform.position; }
        }

        /// <summary>
        /// 碰撞器需要检测的最大半径，超过这个半径则认为不会发生碰撞
        /// </summary>
        internal abstract float MaxRadius
        {
            get;
        }

        /// <summary>
        /// 这个碰撞器是不是碰撞检测器
        /// </summary>
        public bool IsDetector
        {
            get
            {
                return isDetector;
            }
            set
            {
                if (isDetector != value) // 只有有变化时才处理，更新碰撞器成本可以省下来
                {
                    isDetector = value;

                    if (isDetector)
                        Quadtree.AddDetector(this);
                    else
                        Quadtree.RemoveDetector(this);
                }
            }
        }
        [SerializeField]
        private bool isDetector;

        private void Awake()
        {
            // 如果是自动订阅的则将物体上实现了碰撞接口的组件进行订阅
            if (autoSubscribe)
                foreach (Component component in GetComponents<Component>())
                {
                    if (component is IOnQuadtreeCollisionEnter)
                    {
                        collisionEnterEventHandler.AddListener((component as IOnQuadtreeCollisionEnter).OnQuadtreeCollisionEnter);
                    }
                    if (component is IOnQuadtreeCollisionStay)
                    {
                        collisionStayEventHandler.AddListener((component as IOnQuadtreeCollisionStay).OnQuadtreeCollisionStay);
                    }
                    if (component is IOnQuadtreeCollisionExit)
                    {
                        collisionExitEventHandler.AddListener((component as IOnQuadtreeCollisionExit).OnQuadtreeCollisionExit);
                    }
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

        /// <summary>
        /// 发出碰撞事件
        /// </summary>
        /// <param name="collisionColliders"></param>
        internal void SendCollision(List<QuadtreeCollider> collisionColliders)
        {
            // 对所有发生碰撞的物体进行处理
            foreach (QuadtreeCollider collider in collisionColliders)
            {
                // 上一次碰撞检测时没有和这个碰撞器发生碰撞，发出碰撞进入事件
                if (!lastCollisionColliders.Contains(collider))
                {
                    collisionEnterEventHandler?.Invoke(collider);
                }

                // 发出碰撞持续事件
                collisionStayEventHandler?.Invoke(collider);
            }

            // 对上一次碰撞检测的时候发生碰撞，这一检测没有碰撞的碰撞器，发出碰撞离开事件
            foreach (QuadtreeCollider collider in lastCollisionColliders)
            {
                if (!collisionColliders.Contains(collider))
                {
                    collisionExitEventHandler?.Invoke(collider);
                }
            }

            // 记录这一次碰撞检测碰撞到的碰撞器
            lastCollisionColliders = collisionColliders;
        }

        /// <summary>
        /// 如果这个碰撞器与指定碰撞器发生碰撞，返回true
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        public bool IsCollitionToCollider(QuadtreeCollider collider)
        {
            return QuadtreeCollisionDetector.IsCollition(this, collider);
        }

        /// <summary>
        /// 订阅碰撞进入事件
        /// </summary>
        /// <param name=""></param>
        public void SubscribeCollisionEnter(UnityAction<QuadtreeCollider> action)
        {
            //foreach (UnityAction<QuadtreeCollider> subscribedAction in _collisionEnterEventHandler.GetInvocationList())
            //{
            //    if (subscribedAction == action)
            //    {
            //        return;
            //    }
            //}

            collisionEnterEventHandler.AddListener(action);
        }

        /// <summary>
        /// 取消订阅碰撞进入事件
        /// </summary>
        /// <param name=""></param>
        public void CancelSubscribeCollisionEnter(UnityAction<QuadtreeCollider> action)
        {
            //foreach (UnityAction<QuadtreeCollider> subscribedAction in _collisionEnterEventHandler.GetInvocationList())
            //{
            //    if (subscribedAction == action)
            //    {
            //        _collisionEnterEventHandler -= action;
            //        return;
            //    }
            //}

            collisionEnterEventHandler.RemoveListener(action);
        }

        /// <summary>
        /// 订阅碰撞停留事件
        /// </summary>
        /// <param name=""></param>
        public void SubscribeCollisionStay(UnityAction<QuadtreeCollider> action)
        {
            //foreach (UnityAction<QuadtreeCollider> subscribedAction in _collisionStayEventHandler.GetInvocationList())
            //{
            //    if (subscribedAction == action)
            //    {
            //        return;
            //    }
            //}

            collisionStayEventHandler.AddListener(action);
        }

        /// <summary>
        /// 取消订阅碰撞停留事件
        /// </summary>
        /// <param name=""></param>
        public void CancelSubscribeCollisionStay(UnityAction<QuadtreeCollider> action)
        {
            //foreach (UnityAction<QuadtreeCollider> subscribedAction in _collisionStayEventHandler.GetInvocationList())
            //{
            //    if (subscribedAction == action)
            //    {
            //        _collisionStayEventHandler -= action;
            //        return;
            //    }
            //}

            collisionStayEventHandler.RemoveListener(action);
        }

        /// <summary>
        /// 订阅碰撞离开事件
        /// </summary>
        /// <param name=""></param>
        public void SubscribeCollisionExit(UnityAction<QuadtreeCollider> action)
        {
            //foreach (UnityAction<QuadtreeCollider> subscribedAction in _collisionExitEventHandler.GetInvocationList())
            //{
            //    if (subscribedAction == action)
            //    {
            //        return;
            //    }
            //}

            collisionExitEventHandler.AddListener(action);
        }

        /// <summary>
        /// 取消订阅碰撞离开事件
        /// </summary>
        /// <param name=""></param>
        public void CancelSubscribeCollisionExit(UnityAction<QuadtreeCollider> action)
        {
            //foreach (UnityAction<QuadtreeCollider> subscribedAction in _collisionExitEventHandler.GetInvocationList())
            //{
            //    if (subscribedAction == action)
            //    {
            //        _collisionExitEventHandler -= action;
            //        return;
            //    }
            //}

            collisionExitEventHandler.RemoveListener(action);
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
