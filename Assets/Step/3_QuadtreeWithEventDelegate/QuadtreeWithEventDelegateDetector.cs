using UnityEngine;

[RequireComponent(typeof(QuadtreeWithEventDelegateCollider))]      //[RequireComponent(type)]：保证这个脚本挂载时参数脚本也会挂载
public class QuadtreeWithEventDelegateDetector : MonoBehaviour
{
    QuadtreeWithEventDelegateCollider _quadTreeCollider;

    QuadtreeWithEventDelegateCollisionEventDelegate _collisionDelegate;

    private void Awake()
    {
        _quadTreeCollider = GetComponent<QuadtreeWithEventDelegateCollider>();

        _collisionDelegate = new QuadtreeWithEventDelegateCollisionEventDelegate(OnQuadtreeCollision);
    }

    private void OnEnable()
    {
        _quadTreeCollider.collisionEvent += _collisionDelegate;
    }

    private void OnDisable()
    {
        _quadTreeCollider.collisionEvent -= _collisionDelegate;
    }

    void OnQuadtreeCollision(GameObject collisionGameObject)
    {
        Debug.Log(name + "检测到与" + collisionGameObject.name + "发生碰撞");
    }
}
