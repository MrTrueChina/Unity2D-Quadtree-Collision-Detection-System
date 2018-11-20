using System;
using UnityEngine;

public class QuadtreeWithNestedClassCollider : MonoBehaviour
{
    [SerializeField]
    float _radius;
    [SerializeField]
    bool _checkCollision;

    Transform _transform;
    QuadtreeWithNestedClass<GameObject>.Leaf _leaf;     //因为叶子变成了四叉树的内部类，这里就需要通过 四叉树类.叶子类 来表示叶子类


    private void Awake()
    {
        _transform = transform;
        _leaf = new QuadtreeWithNestedClass<GameObject>.Leaf(gameObject, GetLeafPosition(), _radius);
    }
    Vector2 GetLeafPosition()
    {
        return new Vector2(_transform.position.x, _transform.position.y);
    }


    private void OnEnable()
    {
        UpdateLeaf();
        QuadtreeWithNestedClassObject.SetLeaf(_leaf);
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

        GameObject[] colliderGameObjects = QuadtreeWithNestedClassObject.CheckCollision(_leaf);
        foreach (GameObject colliderGameObject in colliderGameObjects)
        {
            if (collisionEvent == null) break;
            collisionEvent(colliderGameObject);
        }
        //每次发出事件进行一次判断，原因是这里循环多次发出事件，但有时候有的组件接到事件后各种操作最后取消了订阅，如果正巧所有订阅都取消了，这里继续循环的时候就会出错，所以要每发出一次判断一次
    }


    private void OnDisable()
    {
        QuadtreeWithNestedClassObject.RemoveLeaf(_leaf);
    }



    private void OnDrawGizmos()
    {
        if (!enabled) return;

        Gizmos.color = _checkCollision ? Color.yellow * 0.8f : Color.green * 0.8f;

        MyGizmos.DrawCircle(transform.position, _radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y), 60);
    }
}
