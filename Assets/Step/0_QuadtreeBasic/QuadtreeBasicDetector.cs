using UnityEngine;


public class QuadtreeBasicDetector : MonoBehaviour
{
    [SerializeField]
    float _radius;

    private void Update()
    {
        GameObject[] objs = QuadtreeBasicObject.CheckCollision(transform.position, _radius);

        foreach (GameObject obj in objs)
            Debug.Log("检测到碰撞器 " + obj.name + " ，位置在 " + obj.transform.position);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow * 0.8f;
        MyGizmos.DrawCircle(transform.position, _radius, 60);
    }
}
