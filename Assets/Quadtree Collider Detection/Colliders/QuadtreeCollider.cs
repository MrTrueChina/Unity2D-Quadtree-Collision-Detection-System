using UnityEngine;

namespace MtC.Tools.QuadtreeCollider
{
    public abstract class QuadtreeCollider : MonoBehaviour
    {
        private Transform _transform;

        private void Awake()
        {
            _transform = transform;
        }

        public Vector2 position
        {
            get { return _transform.position; }
        }

        public abstract float maxRadius
        {
            get;
        }

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

        //TODO：或许可以在订阅列表中通过获取gameObject判断是否被销毁之后清除
    }
}