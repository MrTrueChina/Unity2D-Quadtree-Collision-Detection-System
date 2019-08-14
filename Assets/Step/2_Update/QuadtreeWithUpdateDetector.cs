using System.Collections.Generic;
using UnityEngine;

public class QuadtreeWithUpdateDetector : MonoBehaviour
{
    [SerializeField]
    float _radius;

    List<Transform> _colliders;


    private void OnDrawGizmos()
    {
        DrawCheckRadius();
        DrawCollision();
    }
    void DrawCheckRadius()
    {
        Gizmos.color = Color.yellow * 0.8f;
        MyGizmos.DrawCircle(transform.position, _radius, 60);
    }
    void DrawCollision()
    {
        Gizmos.color = Color.yellow;
        foreach (GameObject collider in QuadtreeWithUpdateObject.CheckCollision(transform.position, _radius))
            Gizmos.DrawLine(transform.position, collider.transform.position);
    }
}
