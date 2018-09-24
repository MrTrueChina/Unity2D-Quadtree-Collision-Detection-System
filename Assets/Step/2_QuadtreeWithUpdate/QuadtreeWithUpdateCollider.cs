using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadtreeWithUpdateCollider : MonoBehaviour
{
    [SerializeField]
    float _radius;
    [SerializeField]
    bool _checkCollision;

    Transform _transform;
    QuadtreeWithUpdateLeaf<GameObject> _leaf;


    private void Awake()
    {
        _transform = transform;
        _leaf = new QuadtreeWithUpdateLeaf<GameObject>(gameObject, GetLeafPosition(), _radius);
    }
    Vector2 GetLeafPosition()
    {
        return new Vector2(_transform.position.x, _transform.position.y);
    }


    private void OnEnable()
    {
        UpdateLeaf();
        QuadtreeWithUpdateObject.SetLeaf(_leaf);
    }


    private void Update()
    {
        UpdateLeaf();

        if (_checkCollision)
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
        GameObject[] colliders = QuadtreeWithUpdateObject.CheckCollision(_leaf);
        foreach (GameObject collider in colliders)
            Debug.Log("碰撞器" + name + "碰到了" + collider.name);
    }


    private void OnDisable()
    {
        QuadtreeWithUpdateObject.RemoveLeaf(_leaf);
    }

    //有三目运算符可能需要解释
    private void OnDrawGizmos()
    {
        Gizmos.color = _checkCollision ? Color.yellow * 0.8f : Color.green * 0.8f;

        MyGizmos.DrawCircle(transform.position, _radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y), 60);
    }
}
