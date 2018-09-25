using UnityEngine;

[RequireComponent(typeof(QuadtreeWithEventDelegateCollider))]
public class QuadtreeWithEventDelegateDetector : MonoBehaviour
{
    QuadtreeWithEventDelegateCollider _quadTreeCollider;

    QuadtreeWithEventDelegateCollisionEventDelegate _collisionDelegate;
    /*
     *  根据委托类型创建的委托，这里只是个字段，因为没法在类里直接用new
     */


    private void Awake()
    {
        _quadTreeCollider = GetComponent<QuadtreeWithEventDelegateCollider>();

        _collisionDelegate = new QuadtreeWithEventDelegateCollisionEventDelegate(OnQuadtreeCollision);
        /*
         *  第三步的前半部分：创建委托
         *  
         *  创建委托和创建类的对象很像，只不过参数是一个方法。确切的说是一个方法名，只有名字，没有返回和参数。
         *  这不是说可以随便传，他的真正意思是：不许你传这些，我来强制定下了。
         *  作为参数的这个方法需要和委托相同的参数和返回，不然通不过编译。
         *  
         *  想想假如这个方法的参数和委托不一样，那么事件发生时就会用错误的参数执行这个方法，各种花式崩溃多么神奇。
         */
    }

    private void OnEnable()
    {
        _quadTreeCollider.collisionEvent += _collisionDelegate;
        /*
         *  第三步后半部分：订阅事件
         *  
         *  说起来你可能不信，前面又是新类型又是传方法做参数的一堆高端操作，最后的订阅竟然是 +=
         *  真的就是那个 += ，事件里面存放的是在事件发生时需要执行的方法，+= 就是把方法存进去。
         *  
         *  事件委托的本质是一个半自动的调用是不是有种又绕回来了的感觉？
         *  现在你知道为什么第二步没订阅的事件直接发出会报错了，因为他本来应该调用方法但没有方法可以调用，于是他就受不了了崩溃了报错了。
         */
    }

    private void OnDisable()
    {
        _quadTreeCollider.collisionEvent -= _collisionDelegate;
        /*
         *  取消订阅，就是 -=
         *  如果不取消订阅的话好像就算是进行订阅的那个对象已经销毁了订阅也还会留在事件里，在无形中浪费掉内存和计算量。
         *  有始有终本来就是代码的重要准则，你总不能因为某个bug只在退出游戏的时候出现就不去管它，那会损害你的程序员之魂的。
         */
    }

    void OnQuadtreeCollision(GameObject collisionGameObject)
    {
        Debug.Log(name + "检测到与" + collisionGameObject.name + "发生碰撞");
    }
}
