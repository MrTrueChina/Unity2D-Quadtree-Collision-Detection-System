using UnityEngine;

public class ColliderMove : MonoBehaviour
{
    public float speed { get; set; }

    void Update ()
    {
        transform.Translate(transform.up * speed * Time.deltaTime);
	}
}
