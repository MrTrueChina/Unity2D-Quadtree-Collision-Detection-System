using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderMove : MonoBehaviour
{
    public float speed { get; set; }
    public float destroyTime { get; set; }


    private void Start()
    {
        Destroy(gameObject, destroyTime);
    }

    void Update ()
    {
        transform.Translate(Vector3.down * speed * Time.deltaTime);
	}
}
