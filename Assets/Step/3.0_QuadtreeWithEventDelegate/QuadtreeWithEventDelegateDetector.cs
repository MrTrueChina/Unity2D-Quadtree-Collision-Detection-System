using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(QuadtreeWithEventDelegateCollider))]
public class QuadtreeWithEventDelegateDetector : MonoBehaviour
{
    QuadtreeWithEventDelegateCollider _quadTreeCollider;

    List<GameObject> _colliders = new List<GameObject>();


    private void Awake()
    {
        _quadTreeCollider = GetComponent<QuadtreeWithEventDelegateCollider>();
    }

    private void OnEnable()
    {
        _quadTreeCollider.collisionEvent += OnQuadtreeCollision;
        /*
         *  第三步：订阅事件
         *  
         *  说起来你可能不信，前面关键字类型互相套的一堆高端操作，最后的订阅竟然是 +=
         *  真的就是那个 += ，事件里面存放的是在事件发生时需要执行的方法，+= 就是把方法存进去。
         *  
         *  事件委托的本质是一个半自动的调用是不是有种又绕回来了的感觉？
         *  现在你知道为什么第二步没订阅的事件直接发出会报错了，因为他本来应该调用方法但没有方法可以调用，于是他就受不了了崩溃了报错了。
         */

        /*
         *  还有一种格式是 _quadTreeCollider.collisionEvent += new QuadtreeWithEventDelegateCollisionEventDelegate(OnQuadtreeCollision);
         *  这是 C#1.0 的格式，直接 +=方法 的是 C#2.0 的格式，官方说两者效果完全相同，这里就用了简单的2.0格式
         */
    }

    private void OnDisable()
    {
        _quadTreeCollider.collisionEvent -= OnQuadtreeCollision;
        /*
         *  取消订阅，就是 -=
         *  如果不取消订阅的话好像就算是进行订阅的那个对象已经销毁了订阅也还会留在事件里，在无形中浪费掉内存和计算量。
         *  有始有终本来就是代码的重要准则，你总不能因为某个bug只在退出游戏的时候出现就不去管它，那会损害你的程序员之魂的。
         */
    }

    void OnQuadtreeCollision(GameObject collisionGameObject)
    {
        _colliders.Add(collisionGameObject);
    }



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach (GameObject collider in _colliders)
            if (collider)                           //从碰撞发生到绘制Gizmo中间有很短的时间，如果在这期间物体被销毁了，就获取不到Trnanform出bug，因此要先判断
                Gizmos.DrawLine(transform.position, collider.transform.position);
        _colliders.Clear();                         //绘制完后清空List，等下一次绘制的时候又会重新填上
    }
}
