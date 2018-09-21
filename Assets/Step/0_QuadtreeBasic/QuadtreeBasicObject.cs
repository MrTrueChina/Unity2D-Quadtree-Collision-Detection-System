using UnityEngine;

public class QuadtreeBasicObject : MonoBehaviour
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
    int _spliceLeafsNumber = 50;
    [SerializeField]
    float _minWidth = 1;
    [SerializeField]
    float _minHeight = 1;

    static QuadtreeBasic<GameObject> _quadtree;


    private void Awake()
    {
        _quadtree = new QuadtreeBasic<GameObject>(_x, _y, _width, _height, _spliceLeafsNumber, _minWidth, _minHeight);
    }


    public static void SetLeaf(QuadtreeBasicLeaf<GameObject> leaf)
    {
        _quadtree.SetLeaf(leaf);
    }

    public static void RemoveLeaf(QuadtreeBasicLeaf<GameObject> leaf)
    {
        _quadtree.RemoveLeaf(leaf);
    }

    public static GameObject[] CheckCollision(Vector2 checkPosition, float radius)
    {
        return _quadtree.CheckCollision(checkPosition, radius);
    }
}
