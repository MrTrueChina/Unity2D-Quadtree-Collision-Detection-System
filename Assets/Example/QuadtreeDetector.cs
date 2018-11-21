using System.Collections.Generic;
using UnityEngine;
using MtC.Tools.Quadtree;


[RequireComponent(typeof(QuadtreeCollider))]
public class QuadtreeDetector : MonoBehaviour
{
    QuadtreeCollider _quadTreeCollider;

    List<Transform> _collisionTransforms = new List<Transform>();


    private void Awake()
    {
        _quadTreeCollider = GetComponent<QuadtreeCollider>();
    }

    private void OnEnable()
    {
        _quadTreeCollider.collisionEvent += OnQuadtreeCollision;
    }

    private void OnDisable()
    {
        _quadTreeCollider.collisionEvent -= OnQuadtreeCollision;
    }

    void OnQuadtreeCollision(GameObject collisionGameObject)
    {
        _collisionTransforms.Add(collisionGameObject.transform);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        foreach (Transform collisionTransform in _collisionTransforms)
            if (collisionTransform)         //绘制前检测碰撞物体是否还存在，因为从碰撞到绘制中间还是有些时间的，这期间可能碰撞物体被销毁了
                Gizmos.DrawLine(collisionTransform.position, transform.position);
        _collisionTransforms.Clear();       //绘制完后清除碰撞物体List，保证每次绘制都是最新的
    }
}
