/*
 *  还是那个四叉树物体，就是加了个更新，还是要设置执行顺序在碰撞器之前。
 */

using UnityEngine;

namespace MtC.Tools.Quadtree.Example.Step2Update
{
    public class QuadtreeObjectUpdate : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        float _top;
#pragma warning disable 0649
        [SerializeField]
        float _right;
#pragma warning disable 0649
        [SerializeField]
        float _bottom;
#pragma warning disable 0649
        [SerializeField]
        float _left;
#pragma warning disable 0649
        [SerializeField]
        int _maxLeafsNumber;
#pragma warning disable 0649
        [SerializeField]
        float _minSideLength;

        static QuadtreeUpdate<GameObject> _quadtree;

        private void Awake()
        {
            _quadtree = new QuadtreeUpdate<GameObject>(_top, _right, _bottom, _left, _maxLeafsNumber, _minSideLength);
        }

        public static bool SetLeaf(QuadtreeLeafUpdate<GameObject> leaf)
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

        public static bool RemoveLeaf(QuadtreeLeafUpdate<GameObject> leaf)
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
