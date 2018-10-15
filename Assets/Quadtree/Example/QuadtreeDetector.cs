/*
 * 检测器，纯演示的。
 * 
 * 要使用碰撞检测就订阅碰撞器的碰撞事件，然后不想用的时候就取消订阅，就这么简单。
 */

using UnityEngine;


[RequireComponent(typeof(QuadtreeCollider))]
public class QuadtreeDetector : MonoBehaviour
{
    QuadtreeCollider _quadTreeCollider;

    QuadtreeCollisionEventDelegate _collisionDelegate;


    private void Awake()
    {
        _quadTreeCollider = GetComponent<QuadtreeCollider>();

        _collisionDelegate = new QuadtreeCollisionEventDelegate(OnQuadtreeCollision);
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
