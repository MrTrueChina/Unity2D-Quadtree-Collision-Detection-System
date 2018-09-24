/*
 *  经过艰难险阻终于到这一步了：四叉树的更新
 *  
 *  前面两步的四叉树都没有更新功能，只要一移动碰撞器就会出错，就是因为缺了一个更新叶子的功能，这一步就来完成这个功能。
 *  
 *  更新分为两部分：叶子位置的更新和叶子半径的更新。
 *  叶子位置的更新是这样的：
 *      首先由根节点发起更新
 *      更新顺着树枝节点递归传递给每一个树梢节点
 *      树梢节点检测所有的叶子的位置
 *          如果叶子还在树梢节点的范围里就不用管它
 *          但如果叶子已经离开了树梢的范围则需要重新从根节点插入叶子，这样叶子就会自动到正确的树梢去
 *          
 *  半径更新是这样的：
 *      还是根节点发出
 *      还是递归到每个树梢
 *      之后树梢要遍历所有叶子，找出新的最大半径
 *          如果这个新的最大半径和原本的最大半径一样那就什么都不用干
 *          但如果这个新的最大半径和原来的最大半径不一样，就要向上更新最大半径
 *          
 *  看了上面两个之后我们可以考虑一下更新顺序：
 *  
 *  是先位置后半径？还是先半径后位置？或者同时进行好像也不错，只用遍历一遍。
 *  
 *  这个坑我已经踩过了，要先更新位置，因为叶子可以从一个树梢的范围移动到另一个树梢的范围，但在位置更新之前所有叶子都被视为在原来的树梢上，这样更新半径的时候就会把半径算到错误的树梢上。
 */
/*
 *  除了增加更新外还增加了一堆 Debug 输出，用来检查各个步骤是不是正确进行的。 
 */

using System.Collections.Generic;
using UnityEngine;


public class QuadtreeWithEventDelegateLeaf<T>
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


    public QuadtreeWithEventDelegateLeaf(T obj, Vector2 position, float radius)
    {
        _obj = obj;
        _position = position;
        _radius = radius;
    }
}


public class QuadtreeWithEventDelegate<T>
{
    Rect _rect;

    float _maxRadius = float.MinValue;

    QuadtreeWithEventDelegate<T> _root;
    /*
     *  根节点，前面已经说到更新位置时需要从根节点再次存入叶子。
     *  虽然可以通过向上递归父节点查到根节点但找个字段存下来计算量小。
     */

    QuadtreeWithEventDelegate<T> _parent;
    QuadtreeWithEventDelegate<T> _upperRightChild;
    QuadtreeWithEventDelegate<T> _lowerRightChild;
    QuadtreeWithEventDelegate<T> _lowerLeftChild;
    QuadtreeWithEventDelegate<T> _upperLeftChild;

    List<QuadtreeWithEventDelegateLeaf<T>> _leafs = new List<QuadtreeWithEventDelegateLeaf<T>>();

    int _maxLeafsNumber;
    float _minWidth;
    float _minHeight;

    //这里有关于三目运算符和根节点需要注释
    public QuadtreeWithEventDelegate(float x, float y, float width, float height, int maxLeafNumber, float minWidth, float minHeight, QuadtreeWithEventDelegate<T> root = null, QuadtreeWithEventDelegate<T> parent = null)
    {
        _rect = new Rect(x, y, width, height);

        _maxLeafsNumber = maxLeafNumber;
        _minWidth = minWidth;
        _minHeight = minHeight;

        _root = root != null ? root : this;

        _parent = parent;
    }


    /*
     *  存入叶子跟上一步一点变化没有，我就复制粘贴改了个类名，连注释都没删
     */
    public bool SetLeaf(QuadtreeWithEventDelegateLeaf<T> leaf)
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

    private bool SetLeafToSelf(QuadtreeWithEventDelegateLeaf<T> leaf)
    {
        _leafs.Add(leaf);
        UpdateMaxRadiusWhenSetLeaf(leaf);
        Debug.Log("<color=#0040A0>位置在" + _rect.position + "宽高是" + _rect.size + "的树梢节点存入位置在" + leaf.position + "半径是" + leaf.radius + "的叶子，存入后的最大半径是" + _maxRadius + "</color>");
        //是的！Log输出同样支持HTML标签，颜色、粗体、斜体等都可以做到
        CheckAndDoSplit();
        return true;
    }
    void UpdateMaxRadiusWhenSetLeaf(QuadtreeWithEventDelegateLeaf<T> leaf)
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
            Debug.Log("<color=#A000A0>位置在" + _rect.position + "宽高是" + _rect.size + "的树枝节点更新最大半径，更新后的最大半径是" + _maxRadius + "</color>");
            CallParentUpdateMaxRadius();
        }
    }
    float GetChildrenMaxRadius()
    {
        return Mathf.Max(_upperRightChild._maxRadius, _lowerRightChild._maxRadius, _lowerLeftChild._maxRadius, _upperLeftChild._maxRadius);
    }

    bool SetLeafToChildren(QuadtreeWithEventDelegateLeaf<T> leaf)
    {
        Debug.Log("<color=#0040A0>位置在" + _rect.position + "宽高是" + _rect.size + "的树枝节点向子节点存入位置在" + leaf.position + "半径是" + leaf.radius + "的叶子</color>");
        if (_upperRightChild._rect.PointToRectDistance(leaf.position) == 0)
            return _upperRightChild.SetLeaf(leaf);
        if (_lowerRightChild._rect.PointToRectDistance(leaf.position) == 0)
            return _lowerRightChild.SetLeaf(leaf);
        if (_lowerLeftChild._rect.PointToRectDistance(leaf.position) == 0)
            return _lowerLeftChild.SetLeaf(leaf);
        if (_upperLeftChild._rect.PointToRectDistance(leaf.position) == 0)
            return _upperLeftChild.SetLeaf(leaf);

        Debug.LogError("向位置在" + _rect.position + "宽高是" + _rect.size + "的节点存入叶子时发生错误：叶子不在所有子节点的范围里。");   //Debug.LogError：在Console面板输出Error，就是红色那种消息
        return false;
    }


    /*
     *  分割多了存入根节点
     */
    void CheckAndDoSplit()
    {
        if (_leafs.Count > _maxLeafsNumber && _rect.width > _minWidth && _rect.height > _minHeight)
            Split();
    }
    void Split()    //对应叶子位置在子节点精度问题造成的夹缝中的极端情况是否需要增加边缘扩展值
    {
        Debug.Log("<color=#808000>位置在" + _rect.position + "宽高是" + _rect.size + "的树梢节点达到分割条件，进行分割</color>");
        float childWidth = _rect.width / 2;
        float childHeight = _rect.height / 2;

        float rightX = _rect.x + childWidth;
        float upperY = _rect.y + childHeight;

        _upperRightChild = new QuadtreeWithEventDelegate<T>(rightX, upperY, childWidth, childHeight, _maxLeafsNumber, _minWidth, _minHeight, _root, this);
        _lowerRightChild = new QuadtreeWithEventDelegate<T>(rightX, _rect.y, childWidth, childHeight, _maxLeafsNumber, _minWidth, _minHeight, _root, this);
        _lowerLeftChild = new QuadtreeWithEventDelegate<T>(_rect.x, _rect.y, childWidth, childHeight, _maxLeafsNumber, _minWidth, _minHeight, _root, this);
        _upperLeftChild = new QuadtreeWithEventDelegate<T>(_rect.x, upperY, childWidth, childHeight, _maxLeafsNumber, _minWidth, _minHeight, _root, this);

        foreach (QuadtreeWithEventDelegateLeaf<T> leaf in _leafs)
            SetLeafToChildren(leaf);
        _leafs = null;
    }


    /*
     *  更新
     */
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
        List<QuadtreeWithEventDelegateLeaf<T>> resetLeafs = new List<QuadtreeWithEventDelegateLeaf<T>>();

        foreach (QuadtreeWithEventDelegateLeaf<T> leaf in _leafs)
            if (_rect.PointToRectDistance(leaf.position) > 0)
                resetLeafs.Add(leaf);

        foreach (QuadtreeWithEventDelegateLeaf<T> leaf in resetLeafs)
            ResetLeaf(leaf);
    }
    void ResetLeaf(QuadtreeWithEventDelegateLeaf<T> leaf)
    {
        Debug.Log("<color=#800080>位置在" + _rect.position + "宽高是" + _rect.size + "的树梢节点移除位置在" + leaf.position + "半径是" + leaf.radius + "的叶子，重新存入树</color>");
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
        float newMaxRadius = float.MinValue;
        foreach (QuadtreeWithEventDelegateLeaf<T> leaf in _leafs)
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
            foreach (QuadtreeWithEventDelegateLeaf<T> leaf in _leafs)
                if (Vector2.Distance(checkPoint, leaf.position) <= checkRadius + leaf.radius)
                    objs.Add(leaf.obj);
        }
        else
        {
            if (_upperRightChild._rect.PointToRectDistance(checkPoint, _maxRadius) <= checkRadius)      //PointToRectDistance的位置在 Quadtree 里
                objs.AddRange(_upperRightChild.CheckCollision(checkPoint, checkRadius));
            if (_lowerRightChild._rect.PointToRectDistance(checkPoint, _maxRadius) <= checkRadius)
                objs.AddRange(_lowerRightChild.CheckCollision(checkPoint, checkRadius));
            if (_lowerLeftChild._rect.PointToRectDistance(checkPoint, _maxRadius) <= checkRadius)
                objs.AddRange(_lowerLeftChild.CheckCollision(checkPoint, checkRadius));
            if (_upperLeftChild._rect.PointToRectDistance(checkPoint, _maxRadius) <= checkRadius)
                objs.AddRange(_upperLeftChild.CheckCollision(checkPoint, checkRadius));
        }
        return objs.ToArray();
    }
    public T[] CheckCollision(QuadtreeWithEventDelegateLeaf<T> leaf)
    {
        List<T> objs = new List<T>(CheckCollision(leaf.position, leaf.radius));
        objs.Remove(leaf.obj);
        return objs.ToArray();
    }



    /*
     *  移除叶子，又是一点变化没有，又是复制粘贴
     */
    public bool RemoveLeaf(QuadtreeWithEventDelegateLeaf<T> leaf)
    {
        if (DontHaveChildren())
            return RemoveLeafSelf(leaf);
        else
            return CallChildrenRemoveLeaf(leaf);
    }
    private bool RemoveLeafSelf(QuadtreeWithEventDelegateLeaf<T> leaf)
    {
        bool removeLeafBool = _leafs.Remove(leaf);
        UpdateMaxRadiusWhenRemoveLeaf();
        Debug.Log("<color=#802030>位置在" + _rect.position + "宽高是" + _rect.size + "的树梢节点移除位置在" + leaf.position + "半径是" + leaf.radius + "的叶子，移除后的最大半径是" + _maxRadius + "</color>");
        return removeLeafBool;
    }
    void UpdateMaxRadiusWhenRemoveLeaf()
    {
        float newMaxRadius = GetLeafsMaxRadiusOnRemoveLeaf();
        if (_maxRadius != newMaxRadius)
        {
            _maxRadius = newMaxRadius;
            Debug.Log("<color=#108010>位置在" + _rect.position + "宽高是" + _rect.size + "的树梢节点半径发生变化，新半径是" + _maxRadius + "</color>");
            CallParentUpdateMaxRadius();
        }
    }
    float GetLeafsMaxRadiusOnRemoveLeaf()
    {
        float newMaxRadius = float.MinValue;

        foreach (QuadtreeWithEventDelegateLeaf<T> leaf in _leafs)
            if (leaf.radius > newMaxRadius)
                if (leaf.radius == _maxRadius)
                    return _maxRadius;
                else
                    newMaxRadius = leaf.radius;

        return newMaxRadius;
    }

    private bool CallChildrenRemoveLeaf(QuadtreeWithEventDelegateLeaf<T> leaf)
    {
        Debug.Log("<color=#802030>位置在" + _rect.position + "宽高是" + _rect.size + "的树枝节点从子节点移除位置在" + leaf.position + "半径是" + leaf.radius + "的叶子</color>");
        if (_upperRightChild._rect.PointToRectDistance(leaf.position) == 0)
            return _upperRightChild.RemoveLeaf(leaf);
        if (_lowerRightChild._rect.PointToRectDistance(leaf.position) == 0)
            return _lowerRightChild.RemoveLeaf(leaf);
        if (_lowerLeftChild._rect.PointToRectDistance(leaf.position) == 0)
            return _lowerLeftChild.RemoveLeaf(leaf);
        if (_upperLeftChild._rect.PointToRectDistance(leaf.position) == 0)
            return _upperLeftChild.RemoveLeaf(leaf);

        Debug.LogError("位置在" + _rect.position + "宽高是" + _rect.size + "的节点，移除叶子失败，叶子不在任何一个子节点的区域里");
        return false;
    }
}
