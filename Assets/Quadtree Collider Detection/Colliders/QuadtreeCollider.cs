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

        //TODO：考虑加一个开关控制是否使用自动配置，使用则发出警告

        private void Awake()
        {
            _transform = transform;
        }

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
                if(_isDetector != value) // 只有有变化时才处理，更新碰撞器成本可以省下来
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

        private void OnEnable()
        {
            Quadtree.AddCollider(this);
        }

        private void OnDisable()
        {
            Quadtree.RemoveCollider(this);
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

        //TODO：或许可以在订阅列表中通过获取gameObject判断是否被销毁之后清除
        //TODO：通过 OnCollisionExit 获取被销毁的碰撞器从原理上不可能，因为已经销毁的碰撞器返回值是null
    }
}
