using UnityEngine;

public class SpownRandomCollider : MonoBehaviour
{
    [SerializeField]
    Vector2 _spownXRange;
    [SerializeField]
    float _spownHeight;
    [SerializeField]
    Vector2 _speedRange;
    [SerializeField]
    Vector2 _destroyTimeRange;
    [SerializeField]
    Vector2 _spownIntervalRange;
    [SerializeField]
    Vector2 _colliderRadiusRange;

    float _nextSpoen;


    private void Update()
    {
        if (Time.time >= _nextSpoen)
        {
            Spown();
            _nextSpoen = Time.time + Random.Range(_spownIntervalRange.x, _spownIntervalRange.y);
        }
    }

    void Spown()
    {
        GameObject collider = new GameObject("Collider");

        collider.transform.position = new Vector3(Random.Range(_spownXRange.x, _spownXRange.y), _spownHeight, 0);

        collider.AddComponent<QuadtreeCollider>();

        collider.transform.localScale *= Random.Range(_colliderRadiusRange.x, _colliderRadiusRange.y);

        ColliderMove move = collider.AddComponent<ColliderMove>();
        move.speed = Random.Range(_speedRange.x, _speedRange.y);
        move.destroyTime = Random.Range(_destroyTimeRange.x, _destroyTimeRange.y);
    }
}
