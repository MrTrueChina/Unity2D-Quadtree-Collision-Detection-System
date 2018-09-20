using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *  这只是个用来演示四叉树效果的脚本，到处都是缺陷，千万不要用到实际工程里
 */
[RequireComponent(typeof(QuadtreeCollider))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    float _moveSpeed;

    Transform _transform;

    private void Awake()
    {
        _transform = GetComponent<Transform>();
        SubscribeCollitionEvent();
    }
    void SubscribeCollitionEvent()
    {
        GetComponent<QuadtreeCollider>().collisionEvent += new CollisionDelegate(Collision);    //如果控制器没了估计物体也没了，碰撞器也没了，暂时就不研究取消订阅了
    }


    private void Update()
    {
        _transform.Translate(new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0) * _moveSpeed * Time.deltaTime);
    }


    void Collision(GameObject collider)
    {
        Debug.Log(gameObject.name + "接触到碰撞器" + collider.name + "，位置在" + collider.transform.position);
    }
}
