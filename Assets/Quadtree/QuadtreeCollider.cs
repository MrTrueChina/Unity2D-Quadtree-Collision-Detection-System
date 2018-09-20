/*
 *  碰撞器，一定要设置执行顺序在 QuadtreeObject 前。
 *  
 *  设置方法是 Edit -> Project Setting -> Script Execution Order，点"+"，之后选择一个脚本，上下拖动，越靠上执行越早，越靠下执行越晚。
 */
using UnityEngine;

public delegate void CollisionDelegate(GameObject obj);

[RequireComponent(typeof(Transform))]
public class QuadtreeCollider : MonoBehaviour
{
    [SerializeField]
    float _radius;
    [SerializeField]
    bool _checkCollision;
    
    Transform _transform;

    QuadtreeLeaf<GameObject> _leaf;


    private void Awake()
    {
        _transform = transform;
        _leaf = new QuadtreeLeaf<GameObject>(gameObject, _transform.position, _radius);
    }

    private void OnEnable()
    {
        UpdateLeaf();
        SetLeafToQuadtree();
    }
    void SetLeafToQuadtree()
    {
        QuadtreeObject.SetLeaf(_leaf);
    }


    private void Update()
    {
        UpdateLeaf();

        if(_checkCollision)
            CheckCollision();
    }
    void UpdateLeaf()
    {
        UpdateLeafPosition();
        UpdateLeafRadius();
    }
    void UpdateLeafPosition()
    {
        _leaf.position = _transform.position;
    }
    void UpdateLeafRadius()
    {
        _leaf.radius = Mathf.Max(_transform.localScale.x, _transform.localScale.y) * _radius;
    }


    public event CollisionDelegate collisionEvent;
    void CheckCollision()
    {
        Debug.Log(gameObject.name + "在" + _transform.position + "，半径" + _radius + "的范围里，与" + QuadtreeObject.CheckCollision(_leaf).Length + "个碰撞器发生碰撞");
        if (collisionEvent == null) return;

        GameObject[] objs = QuadtreeObject.CheckCollision(_leaf);
        foreach (GameObject obj in objs)
            collisionEvent(obj);
    }


    private void OnDisable()
    {
        RemoveLeafFromQuadtree();
    }
    void RemoveLeafFromQuadtree()
    {
        QuadtreeObject.RemoveLeaf(_leaf);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _checkCollision ? Color.yellow * 0.8f : Color.green * 0.8f;

        MyGizmos.DrawCircle(transform.position, _radius * Mathf.Max(transform.localScale.x, transform.localScale.y), 60);
    }
}

public static partial class MyGizmos
{
    public static void DrawCircle(Vector3 center, float radius, int edgeNumber = 360)
    {
        Vector3 beginPoint = center + Vector3.right * radius;       //三角函数角度是从正右方开始的，画圆起始点是最右边的点
        for (int i = 1; i <= edgeNumber; i++)
        {
            float angle = 2 * Mathf.PI / edgeNumber * i;

            float x = radius * Mathf.Cos(angle) + center.x;
            float y = radius * Mathf.Sin(angle) + center.y;
            Vector3 endPoint = new Vector3(x, y, center.z);

            Gizmos.DrawLine(beginPoint, endPoint);

            beginPoint = endPoint;
        }
    }
}