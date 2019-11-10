/*
 *  碰撞器增加了一个碰撞发生的“事件”，用来实现类似Unity的 OnCollision 的效果。
 *  
 *  事件和委托难倒了不少的新人，而且事件委托并不是四叉树的一部分，但她真的太好用了，所以我还是写了这一步。
 *  
 *  直接解释事件委托太困难、太难理解，我们先来看一下Unity的“面向组件”风格，有助于理解事件委托。
 *      
 *      “面向组件”是基于面向对象的编程方式，是为了应对面向对象的大量继承导致的灵活度严重下跌而出现的编程方式。
 *      面向对象的编程思路是把一个对象视为一个整体，所有功能写在一个类里。
 *      面向组件则是把一个对象视为一个组合体，每个功能写成一个类，每个功能可以独立运行。
 *      
 *      面向对象可以解决继承的终极问题：树人问题。
 *      树人这个生物，是植物还是动物？
 *      如果他继承自植物，你需要把动物类里的进食等方法复制一份到树人里面，你违反了DRY（Don't repeat yourself）。
 *      如果继承自动物呢？你需要把植物的光合作用、吸收土壤养分、易燃等写进树人里，你又违反了DRY。
 *      如果继承自生物，你双重违反了DRY。
 *      这个问题根本不能用继承解决，要走接口，而在Unity里的解决方式是：把植物和动物的组件都挂上去。
 *  
 *  了解面向组件后回到碰撞器的问题上
 *  
 *      碰撞器负责检测碰撞，这是只做一件事原则。
 *      你可能想要在碰撞器碰撞时造成减血，或者直接死亡，这并不是碰撞器需要做的事情，这些事情交给生命组件。
 *      之后你可能想要在碰撞时发出闪光，这是个纯视觉效果的功能，与生命值无关。他也可以独立出来，交给碰撞特效组件。
 *      之后你又想要一个在碰撞发生的时候会做一个记录的功能，这样你就能通过数据分析出要不要做些改进。这个功能只在开发过程有用，根本不会发布出去，当然要独立出来，交给碰撞记录组件。
 *      加上记录组件后你又觉得闪光特效太晃眼了要暂时关掉，你只需要把这个组件移除掉就行，等到要用的时候再挂载上去又能正常工作。
 *      
 *      在这种轻松的想要什么就加什么想减什么就减什么的开发方式前面有一个障碍：
 *      生命组件、特效组件、记录组件，他们都是在碰撞发生的时候执行自己的方法，但他们自己没有检测碰撞的方法，这活是碰撞器的。
 *      你要怎么样让碰撞器在每次发生碰撞时都能通知到这些组件呢？或许你可以写下好几个调用，一个个的调用，一个都不漏。
 *      但这之后如果你又想再增加一个发生碰撞时会输出Debug信息的组件你要怎么办？你需要准备一个输出信息组件，还需要在碰撞器里再写一个调用，这违反了开放封闭原则。
 *      一个两个你可以写，如果是三五十个你就会疯掉了，这个问题不能用这么暴力的方法解决。
 *      
 *      一劳永逸的解决方法是有的：
 *      让碰撞器在每次发生碰撞时发出一个消息，他只做这一件事。
 *      需要在碰撞时做出某些行动的组件则来监听这个消息。
 *      碰撞器检测到碰撞 -> 碰撞器发出消息 -> 其他组件收到消息 -> 其他组件开始行动
 *      这样碰撞器只需要写一次，其他组件也只需要准备一个统一的监听消息功能就行。
 *      
 *  要实现这种功能最直接的办法是用C#的事件委托，使用流程大概是这样：
 *  
 *      第一步，你需要定义一个委托类型，他负责联系事件和方法。
 *      第二步，你需要定义一个跟他配对的事件，“配对”听起来有点麻烦其实特别简单，你只要写上要配对的委托类型，C#帮你解决剩下的问题。
 *      第三步，你要进行一个叫“订阅”的操作，就是通过委托把某个方法和事件联系起来，这样事件发出时方法就会执行。
 *      
 *      上面的三步里，前两步在发出事件的组件里，在这一步里就是碰撞器组件。
 *      第三部是在需要订阅事件的组件里写的，在这一步里写在 QuadtreeWithEventDelegateDetector 里
 */

using UnityEngine;

namespace MtC.Tools.Quadtree.Example.Step3Event
{
    public delegate void QuadtreeCollisionEventDelegateEvent(GameObject colliderGameObject);
    /*
     *  这是第一部：定义委托类型。
     *  委托类型看起来像是方法，有返回、有参数，只不过多了个 delegate。
     *  要通过这个委托类型订阅事件的方法必须要有和这个委托类型相同的返回和参数。
     */

    public class QuadtreeColliderEvent : MonoBehaviour
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
        QuadtreeLeafEvent<GameObject> _leaf;

        private void Awake()
        {
            _transform = transform;
            _leaf = new QuadtreeLeafEvent<GameObject>(gameObject, GetLeafPosition(), _radius);
        }
        Vector2 GetLeafPosition()
        {
            return new Vector2(_transform.position.x, _transform.position.y);
        }

        private void OnEnable()
        {
            UpdateLeaf();
            QuadtreeObjectEvent.SetLeaf(_leaf);
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
        public event QuadtreeCollisionEventDelegateEvent collisionEvent;

        /*
         *  第二步：根据前面的委托类型定义的事件，结构是：
         *  event 委托类型 事件名
         */
        void DoCheckCollision()
        {
            if (collisionEvent == null) return;
            /*
             *  首先要检测一下这个事件有没有被订阅，没有订阅的事件是null。
             *  发出没有被订阅的事件卵用没有，谁也不会有行动，实际上你要是真的把没订阅的事件发出了马上就会报错。
             */

            GameObject[] colliderGameObjects = QuadtreeObjectEvent.CheckCollision(_leaf);
            foreach (GameObject colliderGameObject in colliderGameObjects)
            {
                if (collisionEvent == null) break;
                collisionEvent(colliderGameObject);
            }
            /*
             *  发出事件很简单：像方法一样用，名字(参数)
             *  很明显能看出来这里的使用方式和前面定义委托类型的时候的参数和返回是相同的。
             *  
             *  需要注意这里又进行了一次判断，原因是这里循环多次发出事件，但有时候有的组件接到事件后各种操作最后取消了订阅，如果正巧所有订阅都取消了，这里继续循环的时候就会出错，所以要每发出一次判断一次
             */
        }

        private void OnDisable()
        {
            QuadtreeObjectEvent.RemoveLeaf(_leaf);
        }

        private void OnDrawGizmos()
        {
            if (!enabled) return;

            Gizmos.color = _checkCollision ? Color.yellow * 0.8f : Color.green * 0.8f;

            MyGizmos.DrawCircle(transform.position, _radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y), 60);
        }
    }
}
