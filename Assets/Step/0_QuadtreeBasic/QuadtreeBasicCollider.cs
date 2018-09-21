/*
 *  四叉树的碰撞器，挂载在需要参与碰撞检测的物体上
 */

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
        Gizmos.color = Color.green;

        MyGizmos.DrawCircle(transform.position, 0.05f, 20);     //Mygizmos是一个自写的类，位置在 QuadtreeCollider 里，这个方法是画圆圈的
    }
}
