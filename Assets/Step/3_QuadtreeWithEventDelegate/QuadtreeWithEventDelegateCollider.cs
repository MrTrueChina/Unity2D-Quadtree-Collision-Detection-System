/*
 *  碰撞器有很大改变：增加了碰撞检测功能，并且结合委托实现了和Unity的 OnCollider 接近的效果
 */

using UnityEngine;

public delegate void QuadtreeWithEventDelegateCollisionEventDelegate(GameObject colliderGameObject);
/*
 *  这是一个事件委托类型，看起来像是一个方法，有返回、有参数，只不过多了一个 delegate。
 *  他的格式是这样的： 
 *  delegate 返回类型 委托名 (参数)
 *  就是方法前面加一个 delegate
 *  一般来说如果是跨类的委托是定义在类外面的，会加 public
 *  
 *  
 *  关于这个“委托类型”有什么用请看下面这个故事，或者如果你对自己的理解代码的能力有信心的话可以直接跳到这个注释的最后三行。
 *  
 *  有一个可爱的碰撞器，她负责检测碰撞，有很多个组件跟她合作，有帅气的、凶暴的、温柔的、高冷的、智障的以及各种各样的，他们都需要在发生碰撞时做出反应。
 *      先停一下，你觉得这种时候应该怎么办？听起来好像可以给这些组件写调用，全部调用一遍应该就能解决了吧？
 *  
 *  太天真了！这些组件不是随时都到齐的，有时候只有一两个，有时候会有几十个，有时候一个都没有，甚至有时候还会突然出现一个陌生的组件也要碰撞器去通知。
 *  我们可爱的碰撞器面对这群十天有九天半不在的坑爹队友，面对每次都要白找几十个人的痛苦日子，她终于爆发了：“去TMD挨个通知！”
 *      再来停一下，你要面对的是瞬息万变的组件海洋，有时需要运行这个，有时需要运行那个，你根本无法预测到需要运行哪个组件的哪个方法。
 *      你无法把所有可能的组件罗列出来，因为数量庞大到可能要写几万行。
 *      甚至于你就算是全写完了也没有用，需求永远在变化，今天你希望碰撞的一瞬间就算做失败，明天你可能希望增加血量，后天你可能希望在碰撞的同时算一个随机数，没准半个月过去你就希望碰撞的一瞬间十几个组件分别行动演出一场漂亮的焰火晚会。
 *      如果每一次改变都要修改一次碰撞器，你很快就会疯掉，因为这是一个重要的触发点，他是流程的开始，大家都要他来通知，他要不停的改变来适应所有流程。
 *  
 *  回到剧情的最后一段：
 *  可爱的碰撞器终于还是找到了办法：她搞了一台发信机，只要她检测到了碰撞就广播信号，她的任务到此为止。
 *  至于她的坑爹队友们怎么办，当然是想办法接收她广播的信号然后各自行动。
 *  现在我们可爱的碰撞器再也不用浪费时间一个人一个人的通知了，她只要发出一个广播就结束工作了，恭喜她用智慧赢得了舒适的生活。
 *      解决办法很简单：固定住碰撞器，发生碰撞时碰撞器发出消息，其他需要碰撞器的组件则通过接收这个消息进行各种活动。
 *      
 *  
 *  这种故事谁都会讲，每个不会委托的新朋友都听过好几个，并没有什么卵用，只是用来放松的。下面来说一下真正的委托：
 *  
 *  第一步，你需要定义一个委托类型，就是上面那个。
 *  第二步，你需要定义一个跟他配对的事件，“配对”听起来有点麻烦其实特别简单，你只要写上要配对的委托类型，C#帮你解决剩下的问题。这一步在下面有写。
 *  第三步，你要进行一个叫“订阅”的操作，就是通过委托把某个方法和事件联系起来，这样事件发出时方法就会执行。这一步在 QuadtreeWithUpdateDetector 里有写。
 */

public class QuadtreeWithEventDelegateCollider : MonoBehaviour
{
    [SerializeField]
    float _radius;
    [SerializeField]
    bool _checkCollision;

    Transform _transform;
    QuadtreeWithEventDelegateLeaf<GameObject> _leaf;


    private void Awake()
    {
        _transform = transform;
        _leaf = new QuadtreeWithEventDelegateLeaf<GameObject>(gameObject, GetLeafPosition(), _radius);
    }
    Vector2 GetLeafPosition()
    {
        return new Vector2(_transform.position.x, _transform.position.y);
    }


    private void OnEnable()
    {
        UpdateLeaf();
        QuadtreeWithEventDelegateObject.SetLeaf(_leaf);
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
    public event QuadtreeWithEventDelegateCollisionEventDelegate collisionEvent;
    void DoCheckCollision()
    {
        if (collisionEvent == null) return;

        GameObject[] colliderGameObjects = QuadtreeWithEventDelegateObject.CheckCollision(_leaf);
        foreach (GameObject colliderGameObject in colliderGameObjects)
            collisionEvent(colliderGameObject);
    }


    private void OnDisable()
    {
        QuadtreeWithEventDelegateObject.RemoveLeaf(_leaf);
    }

    //有三目运算符可能需要解释
    private void OnDrawGizmos()
    {
        Gizmos.color = _checkCollision ? Color.yellow * 0.8f : Color.green * 0.8f;

        MyGizmos.DrawCircle(transform.position, _radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y), 60);
    }
}
