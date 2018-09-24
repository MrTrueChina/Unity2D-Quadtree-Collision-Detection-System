using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadtreeWithUpdateObject : MonoBehaviour
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

    static QuadtreeWithUpdate<GameObject> _quadtree;


    private void Awake()
    {
        _quadtree = new QuadtreeWithUpdate<GameObject>(_x, _y, _width, _height, _maxLeafsNumber, _minWidth, _minHeight);
    }

    public static bool SetLeaf(QuadtreeWithUpdateLeaf<GameObject> leaf)
    {
        return _quadtree.SetLeaf(leaf);
    }


    private void Update()
    {
        _quadtree.Update();
    }


    public static GameObject[] CheckCollision(QuadtreeWithUpdateLeaf<GameObject> leaf)
    {
        return _quadtree.CheckCollision(leaf);
    }


    public static bool RemoveLeaf(QuadtreeWithUpdateLeaf<GameObject> leaf)
    {
        return _quadtree.RemoveLeaf(leaf);
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
