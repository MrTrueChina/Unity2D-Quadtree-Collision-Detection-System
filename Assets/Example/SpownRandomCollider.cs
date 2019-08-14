using UnityEngine;
using MtC.Tools.Quadtree;

public class SpownRandomCollider : MonoBehaviour
{
    [SerializeField]
    Vector4 _spownField;
    [SerializeField]
    Vector2 _speedRange;
    [SerializeField]
    Vector2 _destroyTimeRange;
    [SerializeField]
    Vector2 _spownIntervalRange;
    [SerializeField]
    Vector2 _colliderRadiusRange;

    float _nextSpown;


    private void Update()
    {
        if (Time.time >= _nextSpown)
        {
            Spown();
            _nextSpown = Time.time + Random.Range(_spownIntervalRange.x, _spownIntervalRange.y);
        }
    }

    void Spown()
    {
        GameObject colliderObject = new GameObject("Collider");
        

        colliderObject.transform.position = new Vector3(Random.Range(_spownField.x, _spownField.z), Random.Range(_spownField.y, _spownField.w), 0);
        

        QuadtreeCollider colliderComponent = colliderObject.AddComponent<QuadtreeCollider>();
        colliderComponent.radius = Random.Range(_colliderRadiusRange.x, _colliderRadiusRange.y);


        float angle = Random.Range(0, 360f);
        colliderObject.transform.eulerAngles = new Vector3(0, 0, angle);


        ColliderMove colliderMove = colliderObject.AddComponent<ColliderMove>();
        colliderMove.speed = Random.Range(_speedRange.x, _speedRange.y);
        Destroy(colliderObject, Random.Range(_destroyTimeRange.x, _destroyTimeRange.y));
    }
}
