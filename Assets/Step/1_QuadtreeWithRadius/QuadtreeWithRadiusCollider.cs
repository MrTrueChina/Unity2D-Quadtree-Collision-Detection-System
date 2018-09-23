using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadtreeWithRadiusCollider : MonoBehaviour
{
    /*
     *  下一步实现类似 OnCollision 的功能，这一步加的东西太多了
     */
    [SerializeField]
    float _radius;

    Transform _transform;
    QuadtreeWithRadiusLeaf<GameObject> _leaf;


    private void Awake()
    {
        _transform = transform;
        _leaf = new QuadtreeWithRadiusLeaf<GameObject>(gameObject, GetLeafPosition(), _radius);
    }
    Vector2 GetLeafPosition()
    {
        return new Vector2(_transform.position.x, _transform.position.y);
    }


    private void OnEnable()
    {
        QuadtreeWithRadiusObject.SetLeaf(_leaf);
    }


    private void OnDisable()
    {
        QuadtreeWithRadiusObject.RemoveLeaf(_leaf);
    }

    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green * 0.8f;

        MyGizmos.DrawCircle(transform.position, _radius * Mathf.Max(transform.localScale.x, transform.localScale.y), 60);
    }
}
