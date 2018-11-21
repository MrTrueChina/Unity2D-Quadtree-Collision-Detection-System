/*
 *  用 Action 代替手写的委托类型
 *  
 *  Action是C#自带的无返回泛型委托，最多支持到16个泛型参数，使用起来是这样的：Action<T1,T2,T3,T....>，每个T按顺序代表了一个参数，比如：
 *  
 *  Action              =   delegate void 委托名()
 *  Action<string>      =   delegate void 委托名(string)
 *  Action<string,int>  =   delegate void 委托名(string, int)
 *  Action<int,string>  =   delegate void 委托名(int, string)
 *  
 *  使用Action可以省略手写委托类型的步骤
 *  
 *  
 *  和 Action 相对的是 Func，Func同样是泛型委托，但他的最后一个泛型参数是返回值，就像这样：
 *  
 *  Func<string>        =   delegate string 委托名()
 *  Func<string,string> =   delegate string 委托名(string)
 *  Func<string,int>    =   delegate int 委托名(string)
 */

using System;
using UnityEngine;

public class QuadtreeWithActionCollider : MonoBehaviour
{
    public float radius
    {
        get { return _radius; }
        set { _radius = value; }
    }
    [SerializeField]
    float _radius = 1;

    public bool checkCollision
    {
        get { return _checkCollision; }
        set { _checkCollision = value; }
    }
    [SerializeField]
    bool _checkCollision;

    Transform _transform;
    QuadtreeWithActionLeaf<GameObject> _leaf;


    private void Awake()
    {
        _transform = transform;
        _leaf = new QuadtreeWithActionLeaf<GameObject>(gameObject, GetLeafPosition(), _radius);
    }
    Vector2 GetLeafPosition()
    {
        return new Vector2(_transform.position.x, _transform.position.y);
    }


    private void OnEnable()
    {
        UpdateLeaf();
        QuadtreeWithActionObject.SetLeaf(_leaf);
    }


    private void Update()
    {
        UpdateLeaf();
        CheckCollision();
    }
    void UpdateLeaf()
    {
        UpdateLeafPosition();
        UpdateLeafRadius();
    }
    void UpdateLeafPosition()
    {
        _leaf.position = GetLeafPosition();
    }
    void UpdateLeafRadius()
    {
        _leaf.radius = Mathf.Max(_transform.lossyScale.x, _transform.lossyScale.y) * _radius;       //注意是 lossyScale 不是localScale，lossyScale 是全局缩放，可以应对父物体缩放后碰撞器一起缩放的情况
    }

    void CheckCollision()
    {
        if (_checkCollision)
            DoCheckCollision();
    }
    public Action<GameObject> collisionEvent;           //在这，用一个GameObject的泛型表示有一个参数是GameObject的委托
    void DoCheckCollision()
    {
        if (collisionEvent == null) return;

        GameObject[] colliderGameObjects = QuadtreeWithActionObject.CheckCollision(_leaf);
        foreach (GameObject colliderGameObject in colliderGameObjects)
        {
            if (collisionEvent == null) break;
            collisionEvent(colliderGameObject);
        }
         //每次发出事件进行一次判断，原因是这里循环多次发出事件，但有时候有的组件接到事件后各种操作最后取消了订阅，如果正巧所有订阅都取消了，这里继续循环的时候就会出错，所以要每发出一次判断一次
    }


    private void OnDisable()
    {
        QuadtreeWithActionObject.RemoveLeaf(_leaf);
    }



    private void OnDrawGizmos()
    {
        if (!enabled) return;

        Gizmos.color = _checkCollision ? Color.yellow * 0.8f : Color.green * 0.8f;

        MyGizmos.DrawCircle(transform.position, _radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y), 60);
    }
}
