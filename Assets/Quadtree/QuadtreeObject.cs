using UnityEngine;

/*
 *  需要设置执行顺序在碰撞器之前
 */
public class QuadtreeObject : MonoBehaviour
{
    [SerializeField]
    float _x = 0;
    [SerializeField]
    float _y = 0;
    [SerializeField]
    float _width = 50;
    [SerializeField]
    float _height = 100;
    [SerializeField]
    int _maxLeafsNumber = 50;
    [SerializeField]
    float _minWidth = 1;
    [SerializeField]
    float _minHeight = 1;

    static Quadtree<GameObject> _quadtree;


    private void Awake()
    {
        _quadtree = new Quadtree<GameObject>(_x, _y, _width, _height, _maxLeafsNumber, _minWidth, _minHeight);
    }


    private void Update()
    {
        _quadtree.Update();
    }


    public static bool SetLeaf(QuadtreeLeaf<GameObject> leaf)
    {
        return _quadtree.SetLeaf(leaf);
    }

    public static bool RemoveLeaf(QuadtreeLeaf<GameObject> leaf)
    {
        return _quadtree.RemoveLeaf(leaf);
    }

    public static GameObject[] CheckCollision(Vector2 checkPosition, float radius)
    {
        return _quadtree.CheckCollision(checkPosition, radius);
    }
    public static GameObject[] CheckCollision(QuadtreeLeaf<GameObject> leaf)
    {
        return _quadtree.CheckCollision(leaf);
    }


    private void OnDrawGizmos()
    {
        Vector3 upperRight = new Vector3(_x + _width, _y + _height, transform.position.z);
        Vector3 lowerRight = new Vector3(_x + _width, _y, transform.position.z);
        Vector3 lowerLeft = new Vector3(_x, _y, transform.position.z);
        Vector3 upperLeft = new Vector3(_x, _y + _height, transform.position.z);

        Gizmos.color = Color.red * 0.8f;

        Gizmos.DrawLine(upperRight, lowerRight);
        Gizmos.DrawLine(lowerRight, lowerLeft);
        Gizmos.DrawLine(lowerLeft, upperLeft);
        Gizmos.DrawLine(upperLeft, upperRight);
    }
}
