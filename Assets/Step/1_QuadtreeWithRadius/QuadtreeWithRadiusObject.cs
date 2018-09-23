using UnityEngine;

public class QuadtreeWithRadiusObject : MonoBehaviour
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

    static QuadtreeWithRadius<GameObject> _quadtree;


    private void Awake()
    {
        _quadtree = new QuadtreeWithRadius<GameObject>(_x, _y, _width, _height, _maxLeafsNumber, _minWidth, _minHeight);
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
