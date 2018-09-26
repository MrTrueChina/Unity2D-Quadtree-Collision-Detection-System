/*
 *  碰撞器，执行顺序要在 QuadtreeObject之后
 *  
 *  设置执行循序的方法：
 *      Edit -> ProjectSettings -> Script Excution Order，打开设置窗口
 *      点"+"，找到要设置的那个脚本，点击
 *      上下拖动，向上是更早执行，向下是更晚执行，没设置的都在 Default Time 里面
 */

using UnityEngine;


public delegate void QuadtreeCollisionEventDelegate(GameObject colliderGameObject);


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
        _leaf = new QuadtreeLeaf<GameObject>(gameObject, GetLeafPosition(), _radius);
    }
    Vector2 GetLeafPosition()
    {
        return new Vector2(_transform.position.x, _transform.position.y);
    }


    private void OnEnable()
    {
        UpdateLeaf();
        QuadtreeObject.SetLeaf(_leaf);
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
    public event QuadtreeCollisionEventDelegate collisionEvent;
    void DoCheckCollision()
    {
        if (collisionEvent == null) return;

        GameObject[] colliderGameObjects = QuadtreeObject.CheckCollision(_leaf);
        foreach (GameObject colliderGameObject in colliderGameObjects)
            collisionEvent(colliderGameObject);
    }


    private void OnDisable()
    {
        QuadtreeObject.RemoveLeaf(_leaf);
    }

    
    private void OnDrawGizmos()
    {
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