/*
 *  四叉树脚本，挂载在需要四叉树碰撞检测的场景里的物体上，设置好数值后就能进行检测了。
 *  
 *  执行顺序要在 QuadtreeCollider 之前
 */

using UnityEngine;

public class QuadtreeWithRadiusObject : MonoBehaviour
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

    static QuadtreeWithRadius<GameObject> _quadtree;


    private void Awake()
    {
        _quadtree = new QuadtreeWithRadius<GameObject>(_top, _right, _bottom, _left, _maxLeafsNumber, _minSideLength);
    }


    public static void SetLeaf(QuadtreeWithRadiusLeaf<GameObject> leaf)
    {
        _quadtree.SetLeaf(leaf);
    }


    public static void RemoveLeaf(QuadtreeWithRadiusLeaf<GameObject> leaf)
    {
        _quadtree.RemoveLeaf(leaf);
    }


    public static GameObject[] CheckCollision(Vector2 checkPosition, float radius)
    {
        return _quadtree.CheckCollision(checkPosition, radius);
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
