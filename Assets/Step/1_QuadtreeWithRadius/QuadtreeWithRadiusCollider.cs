/*
 *  增加了半径的碰撞器，跟第零步的碰撞器基本没区别
 */

using UnityEngine;

public class QuadtreeWithRadiusCollider : MonoBehaviour
{
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
        if (!enabled) return;

        Gizmos.color = Color.green * 0.8f;

        MyGizmos.DrawCircle(transform.position, _radius * Mathf.Max(transform.localScale.x, transform.localScale.y), 60);
    }
}
