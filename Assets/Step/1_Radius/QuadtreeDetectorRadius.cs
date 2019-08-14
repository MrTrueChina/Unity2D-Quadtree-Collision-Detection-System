using UnityEngine;

namespace MtC.Tools.Quadtree.Step.Radius
{
    public class QuadtreeDetectorRadius : MonoBehaviour
    {
        [SerializeField]
        float _radius;

        private void OnDrawGizmos()
        {
            DrawRadius();
            DrawCollision();
        }

        void DrawRadius()
        {
            Gizmos.color = Color.yellow * 0.8f;
            MyGizmos.DrawCircle(transform.position, _radius, 60);
        }

        void DrawCollision()
        {
            foreach (GameObject collider in QuadtreeObjectRadius.CheckCollision(transform.position, _radius))
                Gizmos.DrawLine(transform.position, collider.transform.position);
        }
    }
}
