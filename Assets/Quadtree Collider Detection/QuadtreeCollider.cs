using UnityEngine;

namespace MtC.Tools.QuadtreeCollider.One
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

        private void OnEnable()
        {
            //TODO：加入树
        }

        private void OnDisable()
        {
            //TODO：从树中移除
        }

        //TODO：或许可以在订阅列表中通过获取gameObject判断是否被销毁之后清除
    }
}