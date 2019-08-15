/*
 *  还是那个四叉树物体，就是加了个更新，还是要设置执行顺序在碰撞器之前。
 */

using UnityEngine;

namespace MtC.Tools.Quadtree.Step.Event
{
    public class QuadtreeObjectEvent : MonoBehaviour
    {
        [SerializeField]
        float _top;
        [SerializeField]
        float _right;
        [SerializeField]
        float _bottom;
        [SerializeField]
        float _left;
        [SerializeField]
        int _maxLeafsNumber;
        [SerializeField]
        float _minSideLength;

        static QuadtreeEvent<GameObject> _quadtree;

        private void Awake()
        {
            _quadtree = new QuadtreeEvent<GameObject>(_top, _right, _bottom, _left, _maxLeafsNumber, _minSideLength);
        }

        public static bool SetLeaf(QuadtreeLeafEvent<GameObject> leaf)
        {
            return _quadtree.SetLeaf(leaf);
        }

        /*
         *  每帧更新一次四叉树
         */
        private void Update()
        {
            _quadtree.Update();
        }

        public static GameObject[] CheckCollision(Vector2 checkPoint, float checkRadius)
        {
            return _quadtree.CheckCollision(checkPoint, checkRadius);
        }

        public static GameObject[] CheckCollision(QuadtreeLeafEvent<GameObject> leaf)
        {
            return _quadtree.CheckCollision(leaf);
        }

        public static bool RemoveLeaf(QuadtreeLeafEvent<GameObject> leaf)
        {
            return _quadtree.RemoveLeaf(leaf);
        }

        private void OnDrawGizmos()
        {
            Vector3 upperRight = new Vector3(_right, _top, transform.position.z);
            Vector3 lowerRight = new Vector3(_right, _bottom, transform.position.z);
            Vector3 lowerLeft = new Vector3(_left, _bottom, transform.position.z);
            Vector3 upperLeft = new Vector3(_left, _top, transform.position.z);

            Gizmos.color = Color.red * 0.8f;

            Gizmos.DrawLine(upperRight, lowerRight);
            Gizmos.DrawLine(lowerRight, lowerLeft);
            Gizmos.DrawLine(lowerLeft, upperLeft);
            Gizmos.DrawLine(upperLeft, upperRight);
        }

        private void OnValidate()
        {
            if (_top < _bottom)
                _top = _bottom;
            if (_right < _left)
                _right = _left;
            if (_maxLeafsNumber < 1)
                _maxLeafsNumber = 1;
            if (_minSideLength < 0.001f)
                _minSideLength = 0.001f;
        }
    }
}
