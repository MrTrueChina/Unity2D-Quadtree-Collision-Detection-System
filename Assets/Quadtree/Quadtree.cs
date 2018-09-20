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
        Constructed(obj, position, radius);
    }
    void Constructed(T obj, Vector2 position, float radius)
    {
        _obj = obj;
        _position = position;
        _radius = radius;
    }
}

/*
 *  增加半径：
 *  增加半径必然涉及到严肃问题：移除叶子，之后需要确定节点里最大半径的叶子是谁，之后把他设为最大半径，之后向父级传输
 *  
 *  最简单暴力的方法是移除叶子时做遍历
 *  优化一下：
 *      添加叶子：
 *      果叶子半径比节点最大半径大，换半径，否则不管，也就是 _maxRadius = Mathf.Max(_maxRadius, leaf.radius);
 *      
 *      移除叶子：
 *      如果移除的这个叶子不是最大半径的，不管他
 *      如果是最大半径的，遍历同时记录最大半径，如果遇到同样半径的，结束不作处理，没找到一样半径的，更改最大半径
 *  
 *  换一个思路：二叉堆，用最大堆存放，堆的标志量是半径，堆里的T是List，每次叶子的增删都在二叉堆里做操作，堆顶就是最大半径
 *  
 *  列表法：
 *  每次移除叶子，最快只需1次，最慢遍历全表
 *  每次添加叶子，只需1次比较
 *  堆：
 *  每次移除叶子，可能造成一次堆的调整，只需一次赋值
 *  每次添加叶子，可能造成一次堆调整，一次赋值
 *  
 *  
 *  之后会有更大的问题：更新
 *  首先半径更新需要在移动更新之后进行
 *  如果没有二叉堆，更新就是将所有叶的半径更新，之后遍历出最大半径
 *  
 *  如果有二叉堆
 *  首先按照二叉堆顺序进行是没用的，因为半径的变化带来节点增删，进而堆顺序变化
 *  
 *  列表法：更新每个叶子，一次遍历全表
 *  堆：由于更新过程中的堆调整，可能需要重新建立堆
 */

/*
 *  用列表法
 *  
 *  半径属性
 *  
 *  检测：连半径一起判断
 *  
 *  更新：只按照节点空间分配位置
 */

/*
 *  更新位置、分割也有矛盾
 *  更新位置可能导致分割，分割后有些节点的位置不在范围内
 */

/*
 *  正式版碰撞检测四叉树，存入、更新、检测、移除功能都有了
 */

public class Quadtree<T>
{
    Rect _rect;
    float _maxRadius = float.MinValue;          //最大半径，存入叶子、移除叶子、更新叶子半径时都需要一起向上更新

    Quadtree<T> _parent;
    Quadtree<T> _upperRightChild;
    Quadtree<T> _lowerRightChild;
    Quadtree<T> _lowerLeftChild;
    Quadtree<T> _upperLeftChild;

    List<QuadtreeLeaf<T>> _leafs = new List<QuadtreeLeaf<T>>();

    int _splitNodesNomber;
    float _minWidth;
    float _minHeight;


    //构造
    public Quadtree(float x, float y, float width, float height,int splitLeafsNumber = 10,float minWidth = 1,float minHeight = 1, Quadtree<T> parent = null)
    {
        _rect = new Rect(x, y, width, height);
        _parent = parent;

        _splitNodesNomber = splitLeafsNumber;
        _minWidth = minWidth;
        _minHeight = minHeight;
    }


    //存入
    public bool SetLeaf(QuadtreeLeaf<T> leaf)
    {
        if (_rect.pointToRectDistance(leaf.position) > 0) return false;

        if (DontHaveChildren())
        {
            _leafs.Add(leaf);
            if (_maxRadius < leaf.radius)
            {
                _maxRadius = leaf.radius;
                if (_parent != null)
                    _parent.UpwardsUpdateMaxRaiuds();
            }
            Debug.Log("位置在" + _rect.position + "宽高是" + _rect.size + "的节点，存入一个半径为 " + leaf.radius + " 的叶子，存入后节点最大半径是 " + _maxRadius);

            if (_leafs.Count > _splitNodesNomber && _rect.width > _minWidth && _rect.height > _minHeight)
                Split();

            return true;
        }
        else
        {
            return SetLeafToChild(leaf);
        }
    }
    bool DontHaveChildren()
    {
        return _upperRightChild == null || _lowerRightChild == null || _lowerLeftChild == null || _upperLeftChild == null;      //四个子节点是一起创建的，原理上说一个不存在另外三个也不存在，但假设只有一个不存在插入的叶子又在这个位置就要出事了
    }
    private bool SetLeafToChild(QuadtreeLeaf<T> leaf)
    {
        if (_upperRightChild._rect.pointToRectDistance(leaf.position, 0) == 0)
            return _upperRightChild.SetLeaf(leaf);
        if (_lowerRightChild._rect.pointToRectDistance(leaf.position, 0) == 0)
            return _lowerRightChild.SetLeaf(leaf);
        if (_lowerLeftChild._rect.pointToRectDistance(leaf.position, 0) == 0)
            return _lowerLeftChild.SetLeaf(leaf);
        if (_upperLeftChild._rect.pointToRectDistance(leaf.position, 0) == 0)
            return _upperLeftChild.SetLeaf(leaf);

        Debug.LogError("SetLeafToChild，存入的叶子的位置" + leaf.position + "不在四个子节点的范围里");
        return false;   //缕一下逻辑，执行到这一步的条件是：叶子的位置在这个节点的范围里、这个节点有子节点。四个子节点的范围覆盖父节点的全部范围，也就是说四个if应该有且只有一个通过，假设执行到这一步，说明叶子根本不在这个节点的范围里，存错节点了
    }


    //分割
    void Split()    //对应叶子位置在子节点精度问题造成的夹缝中的极端情况是否需要增加边缘扩展值
    {
        float childWidth = _rect.width / 2;
        float childHeight = _rect.height / 2;

        float rightX = _rect.x + childWidth;
        float upperY = _rect.y + childHeight;

        _upperRightChild = new Quadtree<T>(rightX, upperY, childWidth, childHeight, _splitNodesNomber, _minWidth, _minHeight, this);
        _lowerRightChild = new Quadtree<T>(rightX, _rect.y, childWidth, childHeight, _splitNodesNomber, _minWidth, _minHeight, this);
        _lowerLeftChild = new Quadtree<T>(_rect.x, _rect.y, childWidth, childHeight, _splitNodesNomber, _minWidth, _minHeight, this);
        _upperLeftChild = new Quadtree<T>(_rect.x, upperY, childWidth, childHeight, _splitNodesNomber, _minWidth, _minHeight, this);

        foreach (QuadtreeLeaf<T> leaf in _leafs)
        {
            if (_rect.Contains(leaf.position))
                SetLeafToChild(leaf);
            else
                GetRoot().SetLeaf(leaf);                //有这样一种情况：有的叶子的位置已经移出了节点范围但还没有更新位置，此时分割节点，这些叶子就会超出范围。把这些叶子直接从根节点重新存入
        }
    }


    //检测
    public T[] CheckCollision(Vector2 checkPosition, float checkRadius)
    {
        List<T> objs = new List<T>();

        if (DontHaveChildren())
        {
            foreach (QuadtreeLeaf<T> leaf in _leafs)
                if (Vector2.Distance(leaf.position, checkPosition) <= checkRadius + leaf.radius)
                    objs.Add(leaf.obj);
        }
        else
        {
            if (_upperRightChild._rect.pointToRectDistance(checkPosition, _maxRadius) <= checkRadius)
                objs.AddRange(_upperRightChild.CheckCollision(checkPosition, checkRadius));
            if (_lowerRightChild._rect.pointToRectDistance(checkPosition, _maxRadius) <= checkRadius)
                objs.AddRange(_lowerRightChild.CheckCollision(checkPosition, checkRadius));
            if (_lowerLeftChild._rect.pointToRectDistance(checkPosition, _maxRadius) <= checkRadius)
                objs.AddRange(_lowerLeftChild.CheckCollision(checkPosition, checkRadius));
            if (_upperLeftChild._rect.pointToRectDistance(checkPosition, _maxRadius) <= checkRadius)
                objs.AddRange(_upperLeftChild.CheckCollision(checkPosition, checkRadius));
        }

        return objs.ToArray();
    }
    public T[] CheckCollision(QuadtreeLeaf<T> checkLeaf)
    {
        List<T> objs = new List<T>(CheckCollision(checkLeaf.position, checkLeaf.radius));

        objs.Remove(checkLeaf.obj);

        return objs.ToArray();
    }


    //更新
    public void Update()
    {
        UpdateLeafsPosition();
        UpdateNodesMaxRadius();
    }

    void UpdateLeafsPosition()
    {
        if (DontHaveChildren())
        {
            List<QuadtreeLeaf<T>> resetLeafs = new List<QuadtreeLeaf<T>>();     //如果直接在叶子List里移动，会导致更新到一半List变了，可能导致有的叶子没更新到。在这里先准备一个要移动的叶子的List，之后再按照这个List移动
            foreach (QuadtreeLeaf<T> leaf in _leafs)
                if (_rect.pointToRectDistance(leaf.position, 0) > 0)
                    resetLeafs.Add(leaf);
            foreach (QuadtreeLeaf<T> leaf in resetLeafs)
                ResetLeaf(leaf);
        }
        else
        {
            _upperRightChild.UpdateLeafsPosition();
            _lowerRightChild.UpdateLeafsPosition();
            _lowerLeftChild.UpdateLeafsPosition();
            _upperLeftChild.UpdateLeafsPosition();
        }
    }
    private void ResetLeaf(QuadtreeLeaf<T> leaf)
    {
        Debug.Log("从位置在" + _rect.position + "宽高为" + _rect.size + "的节点移除并重新存入叶子，叶子位置在" + leaf.position);
        RemoveLeaf(leaf);
        GetRoot().SetLeaf(leaf);
    }
    Quadtree<T> GetRoot()
    {
        Quadtree<T> currentNode = this;

        while (currentNode._parent != null)
            currentNode = currentNode._parent;

        return currentNode;
    }

    void UpdateNodesMaxRadius()
    {
        if (DontHaveChildren())
        {
            float newMaxRadius = GetMaxRadius();
            if (newMaxRadius != _maxRadius)
            {
                _maxRadius = newMaxRadius;
                if(_parent != null)
                    _parent.UpwardsUpdateMaxRaiuds();
            }
        }
        else
        {
            _upperRightChild.UpdateNodesMaxRadius();
            _lowerRightChild.UpdateNodesMaxRadius();
            _lowerLeftChild.UpdateNodesMaxRadius();
            _upperLeftChild.UpdateNodesMaxRadius();
        }
    }
    float GetMaxRadius()
    {
        float newMaxRadius = float.MinValue;
        foreach (QuadtreeLeaf<T> leaf in _leafs)
            if (leaf.radius > newMaxRadius)
                newMaxRadius = leaf.radius;
        Debug.Log("计算新半径，新半径 = " + newMaxRadius);
        return newMaxRadius;
    }
    void UpwardsUpdateMaxRaiuds()
    {
        float newMaxRadius = GetChildrenMaxRaius();
        if (newMaxRadius != _maxRadius)
        {
            _maxRadius = newMaxRadius;
            if (_parent != null)
                _parent.UpwardsUpdateMaxRaiuds();
        }
    }
    float GetChildrenMaxRaius()
    {
        return Mathf.Max(_upperRightChild._maxRadius, _lowerRightChild._maxRadius, _lowerLeftChild._maxRadius, _upperLeftChild._maxRadius);
    }


    //移除
    public bool RemoveLeaf(QuadtreeLeaf<T> removeLeaf)
    {
        if (DontHaveChildren())
        {
            bool removebool = _leafs.Remove(removeLeaf);
            ChangeMaxRadiusOnRemoveLeaf(removeLeaf.radius);

            Debug.Log("位置在" + _rect.position + "宽高是" + _rect.size + "的节点，移除一个半径为 " + removeLeaf.radius + " 的叶子，移除后节点最大半径是 " + _maxRadius);

            if (!removebool) Debug.LogError("RmoveLeaf，移除叶子失败，可能是子节点下找不到叶子");
            return removebool;
        }
        else
        {
            if (_upperRightChild._rect.pointToRectDistance(removeLeaf.position) == 0)
                return _upperRightChild.RemoveLeaf(removeLeaf);
            if (_lowerRightChild._rect.pointToRectDistance(removeLeaf.position) == 0)
                return _lowerRightChild.RemoveLeaf(removeLeaf);
            if (_lowerLeftChild._rect.pointToRectDistance(removeLeaf.position) == 0)
                return _lowerLeftChild.RemoveLeaf(removeLeaf);
            if (_upperLeftChild._rect.pointToRectDistance(removeLeaf.position) == 0)
                return _upperLeftChild.RemoveLeaf(removeLeaf);

            Debug.LogError("RemoveLeaf，移除叶子的坐标不在所有子节点范围内");
            return false;
        }
    }
    void ChangeMaxRadiusOnRemoveLeaf(float ramoveLeafRadius)
    {
        float newMaxRadius = 0;
        foreach (QuadtreeLeaf<T> leaf in _leafs)
        {
            if (leaf.radius == _maxRadius) return;  //剩余的叶子里有叶子的最大半径等于现在的最大半径，则说明最大半径不需要调整

            if (leaf.radius > newMaxRadius) newMaxRadius = leaf.radius;
        }
        _maxRadius = newMaxRadius;
    }
}


public static partial class RectExtension
{
    public static float pointToRectDistance(this Rect rect, Vector2 point, float extendedDistance = 0)
    {
        return Mathf.Max(0, GetDistance(GetXDistance(rect, point), GetYDistance(rect, point)) - extendedDistance);
    }
    static float GetXDistance(Rect rect, Vector2 point)
    {
        if (point.x > rect.xMax) return point.x - rect.xMax;

        if (point.x < rect.xMin) return rect.xMin - point.x;

        return 0;
    }
    static float GetYDistance(Rect rect, Vector2 point)
    {
        if (point.y > rect.yMax) return point.y - rect.yMax;

        if (point.y < rect.yMin) return rect.yMin - point.y;

        return 0;
    }
    static float GetDistance(float xDistance, float yDistance)
    {
        return Mathf.Sqrt(xDistance * xDistance + yDistance * yDistance);
    }
}