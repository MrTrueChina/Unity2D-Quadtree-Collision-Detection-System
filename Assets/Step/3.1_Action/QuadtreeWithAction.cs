/*
 *  跟上一步完全一样，区别在 Collider 里
 */

using System.Collections.Generic;
using UnityEngine;


public class QuadtreeWithActionLeaf<T>
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


    public QuadtreeWithActionLeaf(T obj, Vector2 position, float radius)
    {
        _obj = obj;
        _position = position;
        _radius = radius;
    }
}


public class QuadtreeWithAction<T>
{
    QuadtreeWithActionField _field;

    float _maxRadius = Mathf.NegativeInfinity;

    QuadtreeWithAction<T> _root;
    QuadtreeWithAction<T> _parent;
    QuadtreeWithAction<T> _upperRightChild;
    QuadtreeWithAction<T> _lowerRightChild;
    QuadtreeWithAction<T> _lowerLeftChild;
    QuadtreeWithAction<T> _upperLeftChild;

    List<QuadtreeWithActionLeaf<T>> _leafs = new List<QuadtreeWithActionLeaf<T>>();

    int _maxLeafsNumber;
    float _minSideLength;


    public QuadtreeWithAction(float top, float right, float bottom, float left, int maxLeafNumber, float minSideLength, QuadtreeWithAction<T> root = null, QuadtreeWithAction<T> parent = null)
    {
        _field = new QuadtreeWithActionField(top, right, bottom, left);

        _maxLeafsNumber = maxLeafNumber;
        _minSideLength = minSideLength;

        _root = root != null ? root : this;

        _parent = parent;
    }


    public bool SetLeaf(QuadtreeWithActionLeaf<T> leaf)
    {
        if (DontHaveChildren())
            return SetLeafToSelf(leaf);
        else
            return SetLeafToChildren(leaf);
    }
    bool DontHaveChildren()
    {
        return _upperRightChild == null || _lowerRightChild == null || _lowerLeftChild == null || _upperLeftChild == null;      //四个子节点是一起创建的，原理上说一个不存在另外三个也不存在，但假设只有一个不存在插入的叶子又在这个位置就要出事了
    }

    bool SetLeafToSelf(QuadtreeWithActionLeaf<T> leaf)
    {
        _leafs.Add(leaf);
        UpdateMaxRadiusWhenSetLeaf(leaf);
        Debug.Log("<color=#0040A0>位置在" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + "的树梢节点存入位置在" + leaf.position + "半径是" + leaf.radius + "的叶子，存入后的最大半径是" + _maxRadius + "</color>");
        //是的！Log输出同样支持HTML标签，颜色、粗体、斜体等都可以做到
        CheckAndDoSplit();
        return true;
    }
    void UpdateMaxRadiusWhenSetLeaf(QuadtreeWithActionLeaf<T> leaf)
    {
        if (leaf.radius > _maxRadius)       //只有存入的叶子的半径超过了现在节点的最大半径才需要更新最大半径，存入更小的叶子并不会影响到检测。
        {
            _maxRadius = leaf.radius;

            CallParentUpdateMaxRadius();
        }
    }
    void CallParentUpdateMaxRadius()
    {
        if (_parent != null)
            _parent.UpwardUpdateMaxRadius();
    }
    void UpwardUpdateMaxRadius()
    {
        float newManRaiuds = GetChildrenMaxRadius();
        if (newManRaiuds != _maxRadius)
        {
            _maxRadius = newManRaiuds;
            Debug.Log("<color=#A000A0>位置在" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + "的树枝节点更新最大半径，更新后的最大半径是" + _maxRadius + "</color>");
            CallParentUpdateMaxRadius();
        }
    }
    float GetChildrenMaxRadius()
    {
        return Mathf.Max(_upperRightChild._maxRadius, _lowerRightChild._maxRadius, _lowerLeftChild._maxRadius, _upperLeftChild._maxRadius);
    }

    bool SetLeafToChildren(QuadtreeWithActionLeaf<T> leaf)
    {
        Debug.Log("<color=#0040A0>位置在" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + "的树枝节点向子节点存入位置在" + leaf.position + "半径是" + leaf.radius + "的叶子</color>");
        if (_upperRightChild._field.Contains(leaf.position))
            return _upperRightChild.SetLeaf(leaf);
        if (_lowerRightChild._field.Contains(leaf.position))
            return _lowerRightChild.SetLeaf(leaf);
        if (_lowerLeftChild._field.Contains(leaf.position))
            return _lowerLeftChild.SetLeaf(leaf);
        if (_upperLeftChild._field.Contains(leaf.position))
            return _upperLeftChild.SetLeaf(leaf);

        Debug.LogError("向位置在" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + "的节点存入叶子时发生错误：叶子不在所有子节点的范围里。");   //Debug.LogError：在Console面板输出Error，就是红色那种消息
        return false;
    }


    void CheckAndDoSplit()
    {
        if (_leafs.Count > _maxLeafsNumber && _field.width > _minSideLength && _field.height > _minSideLength)
            Split();
    }
    void Split()    //对应叶子位置在子节点精度问题造成的夹缝中的极端情况是否需要增加边缘扩展值
    {
        Debug.Log("<color=#808000>位置在" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + "的树梢节点达到分割条件，进行分割</color>");

        Update();

        float xCenter = (_field.left + _field.right) / 2;
        float yCenter = (_field.bottom + _field.top) / 2;

        _upperRightChild = new QuadtreeWithAction<T>(_field.top, _field.right, yCenter, xCenter, _maxLeafsNumber, _minSideLength, _root, this);
        _lowerRightChild = new QuadtreeWithAction<T>(yCenter, _field.right, _field.bottom, xCenter, _maxLeafsNumber, _minSideLength, _root, this);
        _lowerLeftChild = new QuadtreeWithAction<T>(yCenter, xCenter, _field.bottom, _field.left, _maxLeafsNumber, _minSideLength, _root, this);
        _upperLeftChild = new QuadtreeWithAction<T>(_field.top, xCenter, yCenter, _field.left, _maxLeafsNumber, _minSideLength, _root, this);

        foreach (QuadtreeWithActionLeaf<T> leaf in _leafs)
            SetLeafToChildren(leaf);
        _leafs = null;
    }


    public void Update()
    {
        UpdatePosition();
        UpdateMaxRadius();
    }
    void UpdatePosition()
    {
        if (DontHaveChildren())
            UpdatePositionSelf();
        else
            CallChildrenUpdatePosition();
    }
    void UpdatePositionSelf()
    {
        List<QuadtreeWithActionLeaf<T>> resetLeafs = new List<QuadtreeWithActionLeaf<T>>();

        foreach (QuadtreeWithActionLeaf<T> leaf in _leafs)
            if (!_field.Contains(leaf.position))
                resetLeafs.Add(leaf);

        foreach (QuadtreeWithActionLeaf<T> leaf in resetLeafs)
            ResetLeaf(leaf);
    }
    void ResetLeaf(QuadtreeWithActionLeaf<T> leaf)
    {
        Debug.Log("<color=#800080>位置在" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + "的树梢节点移除位置在" + leaf.position + "半径是" + leaf.radius + "的叶子，重新存入树</color>");
        RemoveLeafSelf(leaf);
        _root.SetLeaf(leaf);
    }
    void CallChildrenUpdatePosition()
    {
        _upperRightChild.UpdatePosition();
        _lowerRightChild.UpdatePosition();
        _lowerLeftChild.UpdatePosition();
        _upperLeftChild.UpdatePosition();
    }

    void UpdateMaxRadius()
    {
        if (DontHaveChildren())
            UpdateMaxRadiusSelf();
        else
            CallChildrenUpdateMaxRadius();
    }
    void UpdateMaxRadiusSelf()
    {
        float newMaxRadius = GetLeafsMaxRadiusOnUpdate();
        if (newMaxRadius != _maxRadius)
        {
            _maxRadius = newMaxRadius;
            CallParentUpdateMaxRadius();
        }
    }
    float GetLeafsMaxRadiusOnUpdate()
    {
        float newMaxRadius = Mathf.NegativeInfinity;
        foreach (QuadtreeWithActionLeaf<T> leaf in _leafs)
            if (leaf.radius > newMaxRadius)
                newMaxRadius = leaf.radius;
        return newMaxRadius;
    }
    void CallChildrenUpdateMaxRadius()
    {
        _upperRightChild.UpdateMaxRadius();
        _lowerRightChild.UpdateMaxRadius();
        _lowerLeftChild.UpdateMaxRadius();
        _upperLeftChild.UpdateMaxRadius();
    }


    /*
     *  碰撞检测，加了一个传叶子检测碰撞的方法，这样碰撞器就可以自己检测碰撞了。
     *  原理很简单，先检测碰撞，之后把叶子自己剔除出去就行了。
     */
    public T[] CheckCollision(Vector2 checkPoint, float checkRadius)
    {
        List<T> objs = new List<T>();
        if (DontHaveChildren())
        {
            foreach (QuadtreeWithActionLeaf<T> leaf in _leafs)
                if (Vector2.Distance(checkPoint, leaf.position) <= checkRadius + leaf.radius)
                    objs.Add(leaf.obj);
        }
        else
        {
            if (_upperRightChild._field.PointToFieldDistance(checkPoint) <= _maxRadius + checkRadius)
                objs.AddRange(_upperRightChild.CheckCollision(checkPoint, checkRadius));
            if (_lowerRightChild._field.PointToFieldDistance(checkPoint) <= _maxRadius + checkRadius)
                objs.AddRange(_lowerRightChild.CheckCollision(checkPoint, checkRadius));
            if (_lowerLeftChild._field.PointToFieldDistance(checkPoint) <= _maxRadius + checkRadius)
                objs.AddRange(_lowerLeftChild.CheckCollision(checkPoint, checkRadius));
            if (_upperLeftChild._field.PointToFieldDistance(checkPoint) <= _maxRadius + checkRadius)
                objs.AddRange(_upperLeftChild.CheckCollision(checkPoint, checkRadius));
        }
        return objs.ToArray();
    }
    public T[] CheckCollision(QuadtreeWithActionLeaf<T> leaf)
    {
        List<T> objs = new List<T>(CheckCollision(leaf.position, leaf.radius));
        objs.Remove(leaf.obj);
        return objs.ToArray();
    }


    //移除，需要增加全树移除
    public bool RemoveLeaf(QuadtreeWithActionLeaf<T> leaf)
    {
        if (DontHaveChildren())
            return RemoveLeafSelf(leaf);
        else
            return CallChildrenRemoveLeaf(leaf);
    }
    bool RemoveLeafSelf(QuadtreeWithActionLeaf<T> leaf)
    {
        if (_leafs.Remove(leaf))
        {
            UpdateMaxRadiusWhenRemoveLeaf();
            Debug.Log("<color=#802030>位置在" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + "的树梢节点移除位置在" + leaf.position + "半径是" + leaf.radius + "的叶子，移除后的最大半径是" + _maxRadius + "</color>");
            return true;
        }

        return _root.RemoveLeafInTotalTree(leaf);
    }
    void UpdateMaxRadiusWhenRemoveLeaf()
    {
        float newMaxRadius = GetLeafsMaxRadiusOnRemoveLeaf();
        if (_maxRadius != newMaxRadius)
        {
            _maxRadius = newMaxRadius;
            Debug.Log("<color=#108010>位置在" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + "的树梢节点半径发生变化，新半径是" + _maxRadius + "</color>");
            CallParentUpdateMaxRadius();
        }
    }
    float GetLeafsMaxRadiusOnRemoveLeaf()
    {
        float newMaxRadius = Mathf.NegativeInfinity;

        foreach (QuadtreeWithActionLeaf<T> leaf in _leafs)
            if (leaf.radius > newMaxRadius)
                if (leaf.radius == _maxRadius)
                    return _maxRadius;
                else
                    newMaxRadius = leaf.radius;

        return newMaxRadius;
    }

    bool CallChildrenRemoveLeaf(QuadtreeWithActionLeaf<T> leaf)
    {
        Debug.Log("<color=#802030>位置在" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + "的树枝节点从子节点移除位置在" + leaf.position + "半径是" + leaf.radius + "的叶子</color>");
        if (_upperRightChild._field.Contains(leaf.position))
            return _upperRightChild.RemoveLeaf(leaf);
        if (_lowerRightChild._field.Contains(leaf.position))
            return _lowerRightChild.RemoveLeaf(leaf);
        if (_lowerLeftChild._field.Contains(leaf.position))
            return _lowerLeftChild.RemoveLeaf(leaf);
        if (_upperLeftChild._field.Contains(leaf.position))
            return _upperLeftChild.RemoveLeaf(leaf);
        return _root.RemoveLeafInTotalTree(leaf);
    }



    bool RemoveLeafInTotalTree(QuadtreeWithActionLeaf<T> leaf)
    {
        if (DontHaveChildren())
        {
            if (_leafs.Remove(leaf))        //List的Remove返回有没有成功从List里移除要移除的元素，元素不存在的时候返回是 false，有了这个返回值就可以非常轻松的判断出这个树梢是不是成功移除了叶子
            {
                UpdateMaxRadiusWhenRemoveLeaf();
                Debug.Log("<color=#802030>位置在" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + "的树梢节点移除位置在" + leaf.position + "半径是" + leaf.radius + "的叶子，移除后的最大半径是" + _maxRadius + "</color>");
                return true;
            }
            return false;
        }
        else
        {
            if (_upperRightChild.RemoveLeafInTotalTree(leaf))
                return true;                                    //如果子节点移除成功了，那就说明不需要继续遍历剩下的节点了，直接返回 true
            if (_lowerRightChild.RemoveLeafInTotalTree(leaf))
                return true;
            if (_lowerLeftChild.RemoveLeafInTotalTree(leaf))
                return true;
            if (_upperLeftChild.RemoveLeafInTotalTree(leaf))
                return true;
            return false;
        }
    }
}



public class QuadtreeWithActionField
{
    public float top
    {
        get { return _top; }
    }
    float _top;
    public float right
    {
        get { return _right; }
    }
    float _right;
    public float bottom
    {
        get { return _bottom; }
    }
    float _bottom;
    public float left
    {
        get { return _left; }
    }
    float _left;
    public float width
    {
        get { return _width; }
    }
    float _width;
    public float height
    {
        get { return _height; }
    }
    float _height;



    public QuadtreeWithActionField(float top, float right, float bottom, float left)
    {
        _top = top;
        _right = right;
        _bottom = bottom;
        _left = left;

        _width = _right - _left;
        _height = _top - _bottom;
    }



    //检测一个点是否在区域里
    public bool Contains(Vector2 point)
    {
        return point.x >= _left && point.x <= _right && point.y >= _bottom && point.y <= _top;
    }



    //计算一个点到区域的距离，如果在区域里则返回0
    public float PointToFieldDistance(Vector2 point)
    {
        float xDistance = Mathf.Max(0, point.x - _right, _left - point.x);
        float yDistance = Mathf.Max(0, point.y - _top, _bottom - point.y);
        return Mathf.Sqrt(xDistance * xDistance + yDistance * yDistance);
    }
}