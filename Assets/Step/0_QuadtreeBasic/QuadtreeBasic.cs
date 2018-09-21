/*
 *  碰撞检测听起来很高端实际上一点科技含量都没有。
 *  用圆形碰撞器举例：碰撞检测就是计算两个碰撞器的圆心距离和半径和哪个大，半径和大就说明碰撞了。
 *  如果是更复杂的碰撞器则可能需要计算边的碰撞，但原理上都逃不脱一个步骤：把所有碰撞器暴力遍历一遍。
 *  
 *  但是纯暴力遍历会有一个很大的问题：假设碰撞器太多了遍历计算量太大怎么办？
 *  于是一种新的思路出现了：先找出可能发生碰撞的碰撞器，之后再遍历。
 *  四叉树就是基于这种思路产生的。
 *  
 *  四叉树通过将空间划分为一个个的小区域来逐步找到真正可能发生碰撞的碰撞器。
 *  四叉树的原理核心在于“分割”：当一个节点的空间里有过多的碰撞器时，就把这个节点分成四个子节点，每个子节点拥有父节点1/4的空间，这个空间里的碰撞器也传给子节点，这样需要遍历的碰撞器就会变少。
 *  
 *  
 *  这个脚本里的是最基础的四叉树，由节点和叶子两部分构成。
 *  节点是四叉树的每个分支，节点组成树本身。
 *  叶子是碰撞器在树立的映射，树通过叶子判断是否发生碰撞。
 *  
 *  除了这个脚本还有一个配合的碰撞器脚本，用来挂载到物体上。
 *  
 *  但需要注意的是有这两个脚本是不够的，缺少一个初始化四叉树的脚本，这两个脚本仅用来帮助理解最后的三个完成品脚本： Quadtree、QuadtreeCollider、QuadtreeObject
 */

using System.Collections.Generic;
using UnityEngine;


public class QuadtreeBasicLeaf<T>
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


    public QuadtreeBasicLeaf(T obj, Vector2 position)
    {
        Constructed(obj, position);
    }
    void Constructed(T obj, Vector2 position)
    {
        _obj = obj;
        _position = position;
    }
}

public class QuadtreeBasic<T>
{
    Rect _rect;
    
    QuadtreeBasic<T> _upperRightChild;
    QuadtreeBasic<T> _lowerRightChild;
    QuadtreeBasic<T> _lowerLeftChild;
    QuadtreeBasic<T> _upperLeftChild;

    List<QuadtreeBasicLeaf<T>> _leafs = new List<QuadtreeBasicLeaf<T>>();

    int _splitNodesNomber;
    float _minWidth;
    float _minHeight;

    public QuadtreeBasic(float x, float y, float width, float height, int splitLeafsNumber = 10, float minWidth = 1, float minHeight = 1)
    {
        _rect = new Rect(x, y, width, height);

        _splitNodesNomber = splitLeafsNumber;
        _minWidth = minWidth;
        _minHeight = minHeight;
    }


    public void SetLeaf(QuadtreeBasicLeaf<T> leaf)
    {
        if (DontHaveChildren())
            SetLeafToSelf(leaf);
        else
            SetLeafToChildren(leaf);
    }
    void SetLeafToSelf(QuadtreeBasicLeaf<T> leaf)
    {
        _leafs.Add(leaf);

        if (_leafs.Count > _splitNodesNomber && _rect.width > _minWidth && _rect.height > _minHeight)
            Split();
    }
    void SetLeafToChildren(QuadtreeBasicLeaf<T> leaf)
    {
        if (_upperRightChild._rect.pointToRectDistance(leaf.position) == 0)
            _upperRightChild.SetLeaf(leaf);
        if (_lowerRightChild._rect.pointToRectDistance(leaf.position) == 0)
            _lowerRightChild.SetLeaf(leaf);
        if (_lowerLeftChild._rect.pointToRectDistance(leaf.position) == 0)
            _lowerLeftChild.SetLeaf(leaf);
        if (_upperLeftChild._rect.pointToRectDistance(leaf.position) == 0)
            _upperLeftChild.SetLeaf(leaf);
    }

    bool DontHaveChildren()
    {
        return _upperRightChild == null || _lowerRightChild == null || _lowerLeftChild == null || _upperLeftChild == null;      //四个子节点是一起创建的，原理上说一个不存在另外三个也不存在，但假设只有一个不存在插入的叶子又在这个位置就要出事了
    }


    void Split()    //对应叶子位置在子节点精度问题造成的夹缝中的极端情况是否需要增加边缘扩展值
    {
        float childWidth = _rect.width / 2;
        float childHeight = _rect.height / 2;

        float rightX = _rect.x + childWidth;
        float upperY = _rect.y + childHeight;

        _upperRightChild = new QuadtreeBasic<T>(rightX, upperY, childWidth, childHeight, _splitNodesNomber, _minWidth, _minHeight);
        _lowerRightChild = new QuadtreeBasic<T>(rightX, _rect.y, childWidth, childHeight, _splitNodesNomber, _minWidth, _minHeight);
        _lowerLeftChild = new QuadtreeBasic<T>(_rect.x, _rect.y, childWidth, childHeight, _splitNodesNomber, _minWidth, _minHeight);
        _upperLeftChild = new QuadtreeBasic<T>(_rect.x, upperY, childWidth, childHeight, _splitNodesNomber, _minWidth, _minHeight);

        foreach (QuadtreeBasicLeaf<T> leaf in _leafs)
            SetLeafToChildren(leaf);
        _leafs = null;
    }


    public T[] CheckCollision(Vector2 checkPosition, float checkRadius)
    {
        List<T> objs = new List<T>();
        if (DontHaveChildren())
        {
            foreach (QuadtreeBasicLeaf<T> leaf in _leafs)
                if (Vector2.Distance(checkPosition, leaf.position) <= checkRadius)
                    objs.Add(leaf.obj);
        }
        else
        {
            if (_upperRightChild._rect.pointToRectDistance(checkPosition) <= checkRadius)
                objs.AddRange(_upperRightChild.CheckCollision(checkPosition, checkRadius));
            if (_lowerRightChild._rect.pointToRectDistance(checkPosition) <= checkRadius)
                objs.AddRange(_lowerRightChild.CheckCollision(checkPosition, checkRadius));
            if (_lowerLeftChild._rect.pointToRectDistance(checkPosition) <= checkRadius)
                objs.AddRange(_lowerLeftChild.CheckCollision(checkPosition, checkRadius));
            if (_upperLeftChild._rect.pointToRectDistance(checkPosition) <= checkRadius)
                objs.AddRange(_upperLeftChild.CheckCollision(checkPosition, checkRadius));
        }
        return objs.ToArray();
    }


    public void RemoveLeaf(QuadtreeBasicLeaf<T> leaf)
    {
        if (DontHaveChildren())
        {
            _leafs.Remove(leaf);
        }
        else
        {
            if (_upperRightChild._rect.pointToRectDistance(leaf.position) == 0)
                _upperRightChild.RemoveLeaf(leaf);
            if (_lowerRightChild._rect.pointToRectDistance(leaf.position) == 0)
                _lowerRightChild.RemoveLeaf(leaf);
            if (_lowerLeftChild._rect.pointToRectDistance(leaf.position) == 0)
                _lowerLeftChild.RemoveLeaf(leaf);
            if (_upperLeftChild._rect.pointToRectDistance(leaf.position) == 0)
                _upperLeftChild.RemoveLeaf(leaf);
        }
    }
}
