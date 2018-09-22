/*
 *  增加了半径的四叉树，同样有配合使用的 QuadtreeWithRadiusObject 和 QuadtreeWithRadiusCollider
 *  
 *  增加了半径后存入、检测、移除的步骤都有所改变
 *  先从检测说起：
 *      叶子没有半径的
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadtreeWithRadiusLeaf<T>
{
    public T obj
    {
        get { return _obj; }
    }
    T _obj;

    public Vector2 position
    {
        get { return _position; }
        set { _position = value; }
    }
    Vector2 _position;

    public float radius
    {
        get { return _radius; }
        set { _radius = value; }
    }
    float _radius;


    public QuadtreeWithRadiusLeaf(T obj, Vector2 position, float radius)
    {
        _obj = obj;
        _position = position;
        _radius = radius;
    }
}

public class QuadtreeWithRadius<T>
{
    Rect _rect;
    float _maxRadius = float.MinValue;
    /*
     *  这个值代表着这个节点里半径最大的那个叶子的半径，节点里没有叶子时设为0或者任何负数都不影响正确性，设为 float 的最小值的原因如下：
     *  
     *  由于这个四叉树在第零步的基础上增加了叶子的半径，碰撞检测向下递归时判断子节点有没有可能有叶子发生碰撞就变得复杂了：
     *  一个节点里可以有好多种半径的叶子，每个叶子可能在任意位置，因此要判断一个子节点里有没有叶子可能发生碰撞的方法变成了 计算检测点到子节点区域的距离，如果这个距离小于测试半径和子节点最大半径的叶子的半径，则说明这个子节点里有可能有叶子会碰撞到。
     *  
     *  根据这个方法，如果一个子节点的最大半径是负数，则判断会发生有趣的现象：检测范围已经覆盖到了这个节点的范围，但因为节点半径是负数，判断上要减去这个值。
     *  那么如果一个子节点的最大半径是float最小值，那么即使测试半径是float的最大值，这个节点也不会被判断为可能发生碰撞。
     *  如果将没有叶子的节点的最大半径设为float最小值，则碰撞检测时这个节点判断为不会发生碰撞，就不会向下迭代这个节点，可以节省一点计算量。
     */
     
    QuadtreeWithRadius<T> _upperRightChild;
    QuadtreeWithRadius<T> _lowerRightChild;
    QuadtreeWithRadius<T> _lowerLeftChild;
    QuadtreeWithRadius<T> _upperLeftChild;

    List<QuadtreeWithRadiusLeaf<T>> _leafs = new List<QuadtreeWithRadiusLeaf<T>>();

    int _maxLeafNumber;
    float _minWidth;
    float _minHeight;


    public QuadtreeWithRadius(float x, float y, float width, float height, int maxLeafNumber, float minWidth, float minHeight)
    {
        _rect = new Rect(x, y, width, height);

        _maxLeafNumber = maxLeafNumber;
        _minWidth = minWidth;
        _minHeight = minHeight;
    }


    /*
     *  增加了半径后存入叶子变得复杂起来：
     *  首先存入叶子后需要更新这个节点的最大半径，但仅仅更新这个节点的最大半径还不够，需要向上一级级的更新最大半径，保证
     */
    public bool SetLeaf(QuadtreeWithRadiusLeaf<T> leaf)
    {
        if (DontHaveChildren())
            
    }
    bool DontHaveChildren()
    {
        return _upperRightChild == null || _lowerRightChild == null || _lowerLeftChild == null || _upperLeftChild == null;      //四个子节点是一起创建的，原理上说一个不存在另外三个也不存在，但假设只有一个不存在插入的叶子又在这个位置就要出事了
    }

    bool SetLeafToSelf(QuadtreeWithRadiusLeaf<T> leaf)
    {
        _leafs.Add(leaf);
    }
}
