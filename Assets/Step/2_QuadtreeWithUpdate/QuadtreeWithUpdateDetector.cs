using UnityEngine;

[RequireComponent(typeof(QuadtreeWithUpdateCollider))]      //[RequireComponent(type)]：保证这个脚本挂载时参数脚本也会挂载
public class QuadtreeWithUpdateDetector : MonoBehaviour
{
    QuadtreeWithUpdateCollider _quadTreeCollider;

    QuadtreeWithUpdateCollisionEventDelegate _collisionDelegate;

    private void Awake()
    {
        _quadTreeCollider = GetComponent<QuadtreeWithUpdateCollider>();

        _collisionDelegate = new QuadtreeWithUpdateCollisionEventDelegate(OnQuadtreeCollision);
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
