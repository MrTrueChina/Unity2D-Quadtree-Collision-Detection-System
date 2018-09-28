/*
 *  正式第二版四叉树
 *  第一版由于实现过于肮脏以至于找不到重构方式惨遭删除。
 *  第二版基于Step3修改而来，删除大部分注释和注释掉Debug输出。如果觉得不需要这些Debug输出的话也可以删掉，删除输出对再次重构可能有帮助。
 *  对各个步骤里都没写的 Rect.PointToRectDistance 单独写注释
 */

using System.Collections.Generic;
using UnityEngine;


public class QuadtreeLeaf<T>
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


    public QuadtreeLeaf(T obj, Vector2 position, float radius)
    {
        _obj = obj;
        _position = position;
        _radius = radius;
    }
}


public class Quadtree<T>
{
    Rect _rect;

    float _maxRadius = Mathf.NegativeInfinity;

    Quadtree<T> _root;
    Quadtree<T> _parent;
    Quadtree<T> _upperRightChild;
    Quadtree<T> _lowerRightChild;
    Quadtree<T> _lowerLeftChild;
    Quadtree<T> _upperLeftChild;

    List<QuadtreeLeaf<T>> _leafs = new List<QuadtreeLeaf<T>>();

    int _maxLeafsNumber;
    float _minWidth;
    float _minHeight;


    public Quadtree(float x, float y, float width, float height, int maxLeafNumber, float minWidth, float minHeight, Quadtree<T> root = null, Quadtree<T> parent = null)
    {
        _rect = new Rect(x, y, width, height);

        _maxLeafsNumber = maxLeafNumber;
        _minWidth = minWidth;
        _minHeight = minHeight;

        _root = root != null ? root : this;

        _parent = parent;
    }


    public bool SetLeaf(QuadtreeLeaf<T> leaf)
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

    private bool SetLeafToSelf(QuadtreeLeaf<T> leaf)
    {
        _leafs.Add(leaf);
        UpdateMaxRadiusWhenSetLeaf(leaf);
        //Debug.Log("<color=#0040A0>位置在" + _rect.position + "宽高是" + _rect.size + "的树梢节点存入位置在" + leaf.position + "半径是" + leaf.radius + "的叶子，存入后的最大半径是" + _maxRadius + "</color>");
        CheckAndDoSplit();
        return true;
    }
    void UpdateMaxRadiusWhenSetLeaf(QuadtreeLeaf<T> leaf)
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
            //Debug.Log("<color=#A000A0>位置在" + _rect.position + "宽高是" + _rect.size + "的树枝节点更新最大半径，更新后的最大半径是" + _maxRadius + "</color>");
            CallParentUpdateMaxRadius();
        }
    }
    float GetChildrenMaxRadius()
    {
        return Mathf.Max(_upperRightChild._maxRadius, _lowerRightChild._maxRadius, _lowerLeftChild._maxRadius, _upperLeftChild._maxRadius);
    }

    bool SetLeafToChildren(QuadtreeLeaf<T> leaf)
    {
        //Debug.Log("<color=#0040A0>位置在" + _rect.position + "宽高是" + _rect.size + "的树枝节点向子节点存入位置在" + leaf.position + "半径是" + leaf.radius + "的叶子</color>");
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


    void CheckAndDoSplit()
    {
        if (_leafs.Count > _maxLeafsNumber && _rect.width > _minWidth && _rect.height > _minHeight)
            Split();
    }
    void Split()    //对应叶子位置在子节点精度问题造成的夹缝中的极端情况是否需要增加边缘扩展值
    {
        //Debug.Log("<color=#808000>位置在" + _rect.position + "宽高是" + _rect.size + "的树梢节点达到分割条件，进行分割</color>");
        float childWidth = _rect.width / 2;
        float childHeight = _rect.height / 2;

        float rightX = _rect.x + childWidth;
        float upperY = _rect.y + childHeight;

        _upperRightChild = new Quadtree<T>(rightX, upperY, childWidth, childHeight, _maxLeafsNumber, _minWidth, _minHeight, _root, this);
        _lowerRightChild = new Quadtree<T>(rightX, _rect.y, childWidth, childHeight, _maxLeafsNumber, _minWidth, _minHeight, _root, this);
        _lowerLeftChild = new Quadtree<T>(_rect.x, _rect.y, childWidth, childHeight, _maxLeafsNumber, _minWidth, _minHeight, _root, this);
        _upperLeftChild = new Quadtree<T>(_rect.x, upperY, childWidth, childHeight, _maxLeafsNumber, _minWidth, _minHeight, _root, this);

        foreach (QuadtreeLeaf<T> leaf in _leafs)
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
        List<QuadtreeLeaf<T>> resetLeafs = new List<QuadtreeLeaf<T>>();

        foreach (QuadtreeLeaf<T> leaf in _leafs)
            if (_rect.PointToRectDistance(leaf.position) > 0)
                resetLeafs.Add(leaf);

        foreach (QuadtreeLeaf<T> leaf in resetLeafs)
            ResetLeaf(leaf);
    }
    void ResetLeaf(QuadtreeLeaf<T> leaf)
    {
        //Debug.Log("<color=#800080>位置在" + _rect.position + "宽高是" + _rect.size + "的树梢节点移除位置在" + leaf.position + "半径是" + leaf.radius + "的叶子，重新存入树</color>");
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
        foreach (QuadtreeLeaf<T> leaf in _leafs)
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


    
    public T[] CheckCollision(Vector2 checkPoint, float checkRadius)
    {
        List<T> objs = new List<T>();
        if (DontHaveChildren())
        {
            foreach (QuadtreeLeaf<T> leaf in _leafs)
                if (Vector2.Distance(checkPoint, leaf.position) <= checkRadius + leaf.radius)
                    objs.Add(leaf.obj);
        }
        else
        {
            if (_upperRightChild._rect.PointToRectDistance(checkPoint, _maxRadius) <= checkRadius)
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
    public T[] CheckCollision(QuadtreeLeaf<T> leaf)
    {
        List<T> objs = new List<T>(CheckCollision(leaf.position, leaf.radius));
        objs.Remove(leaf.obj);
        return objs.ToArray();
    }


    public bool RemoveLeaf(QuadtreeLeaf<T> leaf)
    {
        if (DontHaveChildren())
            return RemoveLeafSelf(leaf);
        else
            return CallChildrenRemoveLeaf(leaf);
    }
    private bool RemoveLeafSelf(QuadtreeLeaf<T> leaf)
    {
        bool removeLeafBool = _leafs.Remove(leaf);
        UpdateMaxRadiusWhenRemoveLeaf();
        //Debug.Log("<color=#802030>位置在" + _rect.position + "宽高是" + _rect.size + "的树梢节点移除位置在" + leaf.position + "半径是" + leaf.radius + "的叶子，移除后的最大半径是" + _maxRadius + "</color>");
        return removeLeafBool;
    }
    void UpdateMaxRadiusWhenRemoveLeaf()
    {
        float newMaxRadius = GetLeafsMaxRadiusOnRemoveLeaf();
        if (_maxRadius != newMaxRadius)
        {
            _maxRadius = newMaxRadius;
            //Debug.Log("<color=#108010>位置在" + _rect.position + "宽高是" + _rect.size + "的树梢节点半径发生变化，新半径是" + _maxRadius + "</color>");
            CallParentUpdateMaxRadius();
        }
    }
    float GetLeafsMaxRadiusOnRemoveLeaf()
    {
        float newMaxRadius = Mathf.NegativeInfinity;

        foreach (QuadtreeLeaf<T> leaf in _leafs)
            if (leaf.radius > newMaxRadius)
                if (leaf.radius == _maxRadius)
                    return _maxRadius;
                else
                    newMaxRadius = leaf.radius;

        return newMaxRadius;
    }

    private bool CallChildrenRemoveLeaf(QuadtreeLeaf<T> leaf)
    {
        //Debug.Log("<color=#802030>位置在" + _rect.position + "宽高是" + _rect.size + "的树枝节点从子节点移除位置在" + leaf.position + "半径是" + leaf.radius + "的叶子</color>");
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



/*
 *  Rect的扩展方法，给Rect增加了计算点到Rect的距离的方法。
 *  
 *  扩展方法：给现有的类增加方法的方法。
 *      扩展方法的结构和普通方法几乎一样，只不过在参数里要增加一个 this 类型 变量名 ，这个参数是什么类型就是谁的扩展方法。
 *      虽然扩展方法是静态方法但他要按照非静态方法的方式来使用：先创建对象，后调用方法，不能通过类名调用。
 *      因为这个写了 this 的参数是直接把调用这个方法的对象拉过来做参数的，通过类名调用就少了一个参数没法运行了。
 *      
 *      写扩展方法需要注意以下几点：
 *          扩展方法必须写在静态类里。
 *          扩展方法必须是静态方法。
 *          扩展方法不能通过类名调用，只能由类的对象调用。
 *          因为不能通过类名调用，所以给静态类写扩展方法是没有用的。
 *          
 *  partial：加了这个的类可以分成好几部分写，可写在一个.cs里也可以写在好几个.cs里，最后在编译的时候编译器会把这个类的各个部分组合到一起编译成一个类。
 *  正好可以用来写扩展类，因为扩展方法必须写在静态类里，所以经常要写一个静态的扩展类来装他，用partial就可以在需要的地方写需要的扩展方法，可读性瞬间提高。
 */
public static partial class RectExtension
{
    public static float PointToRectDistance(this Rect rect, Vector2 point, float extendedDistance = 0)      //参数里的 this Rect rect 代表了这是个扩展方法，调用这个方法的对象在方法里就是 rect
    {
        return Mathf.Max(0, GetDistance(GetXDistance(rect, point), GetYDistance(rect, point)) - extendedDistance);
        /*
         *  extendedDistance这个参数是说把这个rect向周围扩展出去那么长的距离，在第一步 QuadtreeWithRadius 里面写了这个设计的用处。
         *  
         *  真的扩展距离比较麻烦，这里直接在最后减掉扩展距离再限制大于0就解决了。
         */
    }
    static float GetDistance(float xDistance, float yDistance)
    {
        return Mathf.Sqrt(xDistance * xDistance + yDistance * yDistance);   //简单的勾股定理求出距离
    }
    static float GetXDistance(Rect rect, Vector2 point)
    {
        if (point.x > rect.xMax) return point.x - rect.xMax;    //如果点的x坐标比rect的x最大值还要大，说明x在右边（假设x轴正方向是右），点到rect的x轴距离是 x坐标 - rect的x最大值

        if (point.x < rect.xMin) return rect.xMin - point.x;    //在左边则是rect的x最小值 - 点x坐标

        return 0;                                               //既不在左边也不在右边，说明点的x坐标在rect的x轴范围里，距离是0
    }
    static float GetYDistance(Rect rect, Vector2 point)
    {
        if (point.y > rect.yMax) return point.y - rect.yMax;    //y轴同理

        if (point.y < rect.yMin) return rect.yMin - point.y;

        return 0;
    }
}