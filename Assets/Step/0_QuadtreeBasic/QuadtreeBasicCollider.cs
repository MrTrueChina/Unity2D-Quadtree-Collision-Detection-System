using System.Collections;
using System.Collections.Generic;
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
        
    }
}
