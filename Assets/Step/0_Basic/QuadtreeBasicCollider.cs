/*
 *  四叉树的碰撞器，挂载在需要参与碰撞检测的物体上
 */

using UnityEngine;

public class QuadtreeBasicCollider : MonoBehaviour
{
    QuadtreeBasicLeaf<GameObject> _leaf;



    private void Awake()
    {
        _leaf = new QuadtreeBasicLeaf<GameObject>(gameObject, GetLeafPosition());
    }
    Vector2 GetLeafPosition()
    {
        return new Vector2(transform.position.x, transform.position.y);
        /*
         *  阅读下面部分时请将碰撞器想成是固定的不能移动的。
         * 
         *  先来学一个词：
         *  映射：数学术语，指两个元素的集之间元素相互“对应”的关系。
         *  开发里也有“映射”这个概念，碰撞器和叶子的一一对应就是映射，可以说“叶子是碰撞器在四叉树里的映射”。
         * 
         *  新人在遇到“映射”这个概念的时候经常会被绕进去，比如这个四叉树检测的映射就有一个可能搞蒙新人的地方：
         *  四叉树是二维的，Unity的三维的，那么四叉树的xy两个轴对应的是Unity的xy轴还是xz轴呢？也就是说这个四叉树是用在“竖起来”的平面上还是“平放着”的平面上呢？
         *  
         *  实际上四叉树到底是哪个平面是可以设置的。
         *  
         *  首先我们要理解的一点是：
         *  四叉树的平面和Unity的世界空间是分离的，四叉树实际上不知道Unity的世界空间里各个碰撞器到底在哪半径多少，他只能根据存入的叶子来做判断。
         *  这也是为什么在Unity里移动碰撞器检测的结果不会随之变化————四叉树里的叶子没有一起变化，叶子里的数据还是碰撞器刚生成的时候存入的数据。
         *  
         *  理解了这一点后向下就简单了。
         *  
         *  看上面的代码，这个代码将碰撞器的Unity世界坐标的x,y坐标设为叶子的坐标，也就是说四叉树平面的xy轴和Unity的xy轴对应，四叉树平面是竖起来的。
         *  那么如果是把Unity世界坐标的 x,z 设为叶子坐标呢？当然是四叉树平面的xy轴对应Unity的xz轴，四叉树平面是躺倒的。
         *  
         *  也就是说四叉树的平面是什么方向实际上取决于存入叶子的时候的设置，四叉树自己根本不知道Unity的世界里发生了什么，他只会在他自己的二维世界里任劳任怨的完成他的任务。
         *  这种
         *  
         *  
         *  可能有朋友要问了：如果我有的碰撞器是xy，有的碰撞器是xz，有的则是yz，甚至是斜着存的会怎么样？
         *  这个问题接近了映射这个概念的本质了，映射的关键就是“对应”，在这个四叉树里就是把Unity空间里的碰撞器和四叉树里的叶子对应，这样四叉树就可以通过对叶子进行碰撞检测得出碰撞器的碰撞情况。
         *  那么假设有一个叶子和碰撞器没有对应好呢？比如碰撞器已经改变了位置但叶子里记录的位置还是原来的位置。结果当然是检测出错。
         *  
         *  知道了对应关系和对应出错之后再看前面的问题：同一场景里有多种映射规则会怎么样？
         *  答案是：其实也不会怎么样。只要不怕乱尽管用。
         *  
         *  咸鱼作者以前玩过一个游戏，地图是上下两部分，下面的部分是倒影，有意思的是上下两部分都只显示一部分物体，两边加起来才是真的地图，这种功能就可以上面的物体正着映射，下面的物体反着映射。
         *  开发本来就是随意发挥成分很多的，只要自己控制得住就尽管放手去用。
         *  
         *  万一玩脱了不要找咸鱼作者撒气→_→
         */
    }


    /*
     *  在 OnEnble 和 OnDisable 里写存入和移除叶子方法。
     *  OnEnable 是Unity自带的在脚本激活的时候自动调用的方法，脚本激活后向四叉树存入叶子，碰撞器生效。
     *  OnDisable 则是在脚本禁用的时候自动调用的，脚本禁用后从四叉树里移除叶子，碰撞器失效。
     *  
     *  一个脚本实现一个功能，脚本激活则功能生效，脚本禁用则功能失效，这是面向组件的编程方式，也是Unity所使用的方式，这段话主要是给新朋友写的，老手看了别笑话我。
     */
    private void OnEnable()
    {
        QuadtreeBasicObject.SetLeaf(_leaf);
    }


    private void OnDisable()
    {
        QuadtreeBasicObject.RemoveLeaf(_leaf);
    }


    //关于 OnDrawGizmos 请看 QuadtreeBaiscObject
    private void OnDrawGizmos()
    {
        if (!enabled) return;                               //在更新到正式第二版之后才发现OnDrawGizmos就算是停用了组件也会运行，于是加上这个组件停用直接返回

        Gizmos.color = Color.green;

        MyGizmos.DrawCircle(transform.position, 10, 20);    //Mygizmos是一个自写的类，位置在 QuadtreeCollider 里，这个方法是画圆圈的
    }
}
