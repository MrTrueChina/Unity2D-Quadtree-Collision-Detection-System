using System.Collections;
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
        {
            _leafs.Add(leaf);

            if (_leafs.Count > _splitNodesNomber && _rect.width > _minWidth && _rect.height > _minHeight)
                Split();
        }
        else
        {
            
        }
    }
    bool DontHaveChildren()
    {
        return _upperRightChild == null || _lowerRightChild == null || _lowerLeftChild == null || _upperLeftChild == null;      //四个子节点是一起创建的，原理上说一个不存在另外三个也不存在，但假设只有一个不存在插入的叶子又在这个位置就要出事了
    }


    void Split()
    {

    }


    public void RemoveLeaf()
    {

    }
}
