using System;
using UnityEngine;

public class QuadtreeCollider : MonoBehaviour
{
    public float radius
    {
        get { return _radius; }
        set { _radius = value; }
    }
    [SerializeField]
    float _radius = 1;

    public bool checkCollision
    {
        get { return _checkCollision; }
        set { _checkCollision = value; }
    }
    [SerializeField]
    bool _checkCollision;

    Transform _transform;
    QuadtreeData<GameObject>.Leaf _leaf;


    private void Awake()
    {
        _transform = transform;
        _leaf = new QuadtreeData<GameObject>.Leaf(gameObject, GetLeafPosition(), _radius);
    }
    Vector2 GetLeafPosition()
    {
        return new Vector2(_transform.position.x, _transform.position.y);
    }


    private void OnEnable()
    {
        UpdateLeaf();
        Quadtree.SetLeaf(_leaf);
    }


    private void Update()
    {
        UpdateLeaf();
        CheckCollision();
    }
    void UpdateLeaf()
    {
        UpdateLeafPosition();
        UpdateLeafRadius();
    }
    void UpdateLeafPosition()
    {
        _leaf.position = GetLeafPosition();
    }
    void UpdateLeafRadius()
    {
        _leaf.radius = Mathf.Max(_transform.lossyScale.x, _transform.lossyScale.y) * _radius;       //注意是 lossyScale 不是localScale，lossyScale 是全局缩放，可以应对父物体缩放后碰撞器一起缩放的情况
    }

    void CheckCollision()
    {
        if (_checkCollision)
            DoCheckCollision();
    }
    public Action<GameObject> collisionEvent;
    void DoCheckCollision()
    {
        if (collisionEvent == null) return;

        GameObject[] colliderGameObjects = Quadtree.CheckCollision(_leaf);
        foreach (GameObject colliderGameObject in colliderGameObjects)
        {
            if (collisionEvent == null) break;
            collisionEvent(colliderGameObject);
        }
        //每次发出事件进行一次判断，原因是这里循环多次发出事件，但有时候有的组件接到事件后各种操作最后取消了订阅，如果正巧所有订阅都取消了，这里继续循环的时候就会出错，所以要每发出一次判断一次
    }


    private void OnDisable()
    {
        Quadtree.RemoveLeaf(_leaf);
    }



    private void OnDrawGizmos()
    {
        if (!enabled) return;

        Gizmos.color = _checkCollision ? Color.yellow * 0.8f : Color.green * 0.8f;

        MyGizmos.DrawCircle(transform.position, _radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y), 60);
    }
}



/*
 *  Unity自带的Gizmos方法功能很少，要绘制出更多的Gizmo就要自写
 *  
 *  partical在 Quadtree 里写了
 */
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