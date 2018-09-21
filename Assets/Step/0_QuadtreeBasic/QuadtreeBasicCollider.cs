using UnityEngine;

public class QuadtreeBasicCollider : MonoBehaviour
{
    Transform _transform;
    QuadtreeBasicLeaf<GameObject> _leaf;


    private void Awake()
    {
        _transform = transform;
        _leaf = new QuadtreeBasicLeaf<GameObject>(gameObject, _transform.position);
    }


    private void OnEnable()
    {
        QuadtreeBasicObject.SetLeaf(_leaf);
    }


    private void OnDisable()
    {
        QuadtreeBasicObject.RemoveLeaf(_leaf);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawSphere(transform.position, 0.1f);
    }
}
