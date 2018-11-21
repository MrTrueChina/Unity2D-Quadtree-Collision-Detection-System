/*
 *  正式第三版四叉树，在第六步的基础上改造后得来
 *  
 *  先在
 */

using System.Collections.Generic;
using UnityEngine;

public class Quadtree : MonoBehaviour
{
    static Quadtree quadtreeObject
    {
        get
        {
            if (_quadtreeObject != null)
                return _quadtreeObject;

            _quadtreeObject = new GameObject("Quadtree").AddComponent<Quadtree>();
            return _quadtreeObject;
        }
    }
    static Quadtree _quadtreeObject;

    QuadtreeData<GameObject> _quadtree;



    //初始化
    private void Awake()
    {
        QuadtreeSetting setting = Resources.Load<QuadtreeSetting>("QuadtreeSetting");
        _quadtree = new QuadtreeData<GameObject>(setting.top, setting.right, setting.bottom, setting.left, setting.maxLeafsNumber, setting.minSideLength);
    }



    //存入
    public static void SetLeaf(QuadtreeData<GameObject>.Leaf leaf)
    {
        quadtreeObject._quadtree.SetLeaf(leaf);
    }



    //更新
    private void Update()
    {
        _quadtree.Update();
    }



    //检测
    public static GameObject[] CheckCollision(Vector2 checkPoint, float checkRadius)
    {
        if (_quadtreeObject != null)
            return quadtreeObject._quadtree.CheckCollision(checkPoint, checkRadius);
        return new GameObject[0];
    }
    public static GameObject[] CheckCollision(QuadtreeData<GameObject>.Leaf leaf)
    {
        if (_quadtreeObject != null)
            return quadtreeObject._quadtree.CheckCollision(leaf);
        return new GameObject[0];
    }



    //移除
    public static bool RemoveLeaf(QuadtreeData<GameObject>.Leaf leaf)
    {
        if (_quadtreeObject != null)
            return _quadtreeObject._quadtree.RemoveLeaf(leaf);
        return false;
    }
}


public class QuadtreeData<T>
{
    public class Leaf
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


        public Leaf(T obj, Vector2 position, float radius)
        {
            _obj = obj;
            _position = position;
            _radius = radius;
        }
    }
    class Field
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

        public Vector2 center       //center好像只在向上生长的时候才会使用到一次，考虑之后觉得还是用查询吧，反正都是只用一次，节省一点内存
        {
            get
            {
                return new Vector2((_left + _right) / 2, (_bottom + _top) / 2);
            }
        }



        public Field(float top, float right, float bottom, float left)
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


    Field _field;

    float _maxRadius = Mathf.NegativeInfinity;

    QuadtreeData<T> _root;
    QuadtreeData<T> _parent;
    QuadtreeData<T> _upperRightChild;
    QuadtreeData<T> _lowerRightChild;
    QuadtreeData<T> _lowerLeftChild;
    QuadtreeData<T> _upperLeftChild;

    List<Leaf> _leafs = new List<Leaf>();

    int _maxLeafsNumber;
    float _minSideLength;



    public QuadtreeData(float top, float right, float bottom, float left, int maxLeafNumber, float minSideLength, QuadtreeData<T> root = null, QuadtreeData<T> parent = null)
    {
        _field = new Field(top, right, bottom, left);

        _maxLeafsNumber = maxLeafNumber;
        _minSideLength = minSideLength;

        _root = root != null ? root : this;

        _parent = parent;

        DrawField();    //绘制节点范围，删除不影响功能
    }



    //存入
    public bool SetLeaf(Leaf leaf)
    {
        if (_root._field.Contains(leaf.position))
            return _root.DoSetLeaf(leaf);
        else
        {
            _root.UpwardGrouth(leaf.position);
            SetLeaf(leaf);
        }
        return false;
    }
    bool DoSetLeaf(Leaf leaf)
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

    bool SetLeafToSelf(Leaf leaf)
    {
        _leafs.Add(leaf);
        UpdateMaxRadiusWhenSetLeaf(leaf);
        CheckAndDoSplit();
        return true;
    }
    void UpdateMaxRadiusWhenSetLeaf(Leaf leaf)
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
            CallParentUpdateMaxRadius();
        }
    }
    float GetChildrenMaxRadius()
    {
        return Mathf.Max(_upperRightChild._maxRadius, _lowerRightChild._maxRadius, _lowerLeftChild._maxRadius, _upperLeftChild._maxRadius);
    }

    bool SetLeafToChildren(Leaf leaf)
    {
        if (_upperRightChild._field.Contains(leaf.position))
            return _upperRightChild.DoSetLeaf(leaf);
        if (_lowerRightChild._field.Contains(leaf.position))
            return _lowerRightChild.DoSetLeaf(leaf);
        if (_lowerLeftChild._field.Contains(leaf.position))
            return _lowerLeftChild.DoSetLeaf(leaf);
        if (_upperLeftChild._field.Contains(leaf.position))
            return _upperLeftChild.DoSetLeaf(leaf);

        //增加反向生长后正常情况下应该不会走到这一步
        Debug.LogError("向位置在" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + "的节点存入叶子时发生错误：叶子不在所有子节点的范围里。");
        return false;
    }


    void CheckAndDoSplit()
    {
        if (_leafs.Count > _maxLeafsNumber && _field.width > _minSideLength && _field.height > _minSideLength)
            Split();
    }
    void Split()
    {
        DoUpdate();

        float xCenter = (_field.left + _field.right) / 2;
        float yCenter = (_field.bottom + _field.top) / 2;

        _upperRightChild = new QuadtreeData<T>(_field.top, _field.right, yCenter, xCenter, _maxLeafsNumber, _minSideLength, _root, this);
        _lowerRightChild = new QuadtreeData<T>(yCenter, _field.right, _field.bottom, xCenter, _maxLeafsNumber, _minSideLength, _root, this);
        _lowerLeftChild = new QuadtreeData<T>(yCenter, xCenter, _field.bottom, _field.left, _maxLeafsNumber, _minSideLength, _root, this);
        _upperLeftChild = new QuadtreeData<T>(_field.top, xCenter, yCenter, _field.left, _maxLeafsNumber, _minSideLength, _root, this);

        foreach (Leaf leaf in _leafs)
            SetLeafToChildren(leaf);
        _leafs = null;
    }



    //向上生长
    void UpwardGrouth(Vector2 leafPosition)
    {
        /* 
         *  先要明确什么情况下向哪个方向生长
         *  
         *  以原范围中心点为基准点
         *  如果叶子在基准点左，向左生长，如果在基准点位置或右边，向右生长
         *  如果叶子在基准点下方，向下生长，如果在基准点位置或上方，向上生长
         */

        Vector2 growthDirection = leafPosition - _field.center;     //方向，正数是上和右

        float newTop = growthDirection.y >= 0 ? _field.top + _field.height : _field.top;
        float newRight = growthDirection.x >= 0 ? _field.right + _field.width : _field.right;
        float newBottom = growthDirection.y >= 0 ? _field.bottom : _field.bottom - _field.height;
        float newLeft = growthDirection.x >= 0 ? _field.left : _field.left - _field.width;
        float newXCenter = growthDirection.x >= 0 ? _field.right : _field.left;
        float newYCenter = growthDirection.y >= 0 ? _field.top : _field.bottom;

        QuadtreeData<T> newRoot = new QuadtreeData<T>(newTop, newRight, newBottom, newLeft, _maxLeafsNumber, _minSideLength);      //新根节点

        //右上节点，需要存入的情况是向左下方生长，即 x < 0 && y < 0
        if (growthDirection.x >= 0 || growthDirection.y >= 0)       //只要不满足向左下方生长的条件就用创建
            newRoot._upperRightChild = new QuadtreeData<T>(newTop, newRight, newYCenter, newXCenter, _maxLeafsNumber, _minSideLength, newRoot, newRoot);
        else
            newRoot._upperRightChild = this;

        //右下节点，需要存入的情况是向左上方生长，即 x <0 && y >= 0
        if (growthDirection.x >= 0 || growthDirection.y < 0)
            newRoot._lowerRightChild = new QuadtreeData<T>(newYCenter, newRight, newBottom, newXCenter, _maxLeafsNumber, _minSideLength, newRoot, newRoot);
        else
            newRoot._lowerRightChild = this;

        //左下节点，需要存入的情况是向右上方生长，即 x >= 0 && y >= 0
        if (growthDirection.x < 0 || growthDirection.y < 0)
            newRoot._lowerLeftChild = new QuadtreeData<T>(newYCenter, newXCenter, newBottom, newLeft, _maxLeafsNumber, _minSideLength, newRoot, newRoot);
        else
            newRoot._lowerLeftChild = this;

        //左上节点，需要存入的情况是向右下方生长，即 x >= 0 && y < 0
        if (growthDirection.x < 0 || growthDirection.y >= 0)
            newRoot._upperLeftChild = new QuadtreeData<T>(newTop, newXCenter, newYCenter, newLeft, _maxLeafsNumber, _minSideLength, newRoot, newRoot);
        else
            newRoot._upperLeftChild = this;

        _parent = newRoot;              //因为每次向上生长都是由现在的根节点调用的，新的根节点生长完成后旧的根节点的父节点就是新的根节点
        newRoot.UpdateRoot(newRoot);
    }
    void UpdateRoot(QuadtreeData<T> root)
    {
        _root = root;
        if (!DontHaveChildren())
        {
            _upperRightChild.UpdateRoot(root);
            _lowerRightChild.UpdateRoot(root);
            _lowerLeftChild.UpdateRoot(root);
            _upperLeftChild.UpdateRoot(root);
        }
    }



    //更新
    public void Update()
    {
        _root.DoUpdate();
    }
    void DoUpdate()
    {
        UpdatePosition();
        UpdateMaxRadius();
    }
    void UpdatePosition()
    {
        if (DontHaveChildren())
            UpdateSelfPosition();
        else
            UpdateChildrensPosition();
    }
    void UpdateSelfPosition()
    {
        List<Leaf> resetLeafs = new List<Leaf>();

        foreach (Leaf leaf in _leafs)
            if (!_field.Contains(leaf.position))
                resetLeafs.Add(leaf);

        foreach (Leaf leaf in resetLeafs)
            ResetLeaf(leaf);
    }
    void ResetLeaf(Leaf leaf)
    {
        RemoveLeafFromSelf(leaf);
        _root.SetLeaf(leaf);
    }
    void UpdateChildrensPosition()
    {
        _upperRightChild.UpdatePosition();
        _lowerRightChild.UpdatePosition();
        _lowerLeftChild.UpdatePosition();
        _upperLeftChild.UpdatePosition();
    }

    void UpdateMaxRadius()
    {
        if (DontHaveChildren())
            UpdateSelfMaxRadius();
        else
            UpdateChildrensMaxRadius();
    }
    void UpdateSelfMaxRadius()
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
        foreach (Leaf leaf in _leafs)
            if (leaf.radius > newMaxRadius)
                newMaxRadius = leaf.radius;
        return newMaxRadius;
    }
    void UpdateChildrensMaxRadius()
    {
        _upperRightChild.UpdateMaxRadius();
        _lowerRightChild.UpdateMaxRadius();
        _lowerLeftChild.UpdateMaxRadius();
        _upperLeftChild.UpdateMaxRadius();
    }



    //检测
    public T[] CheckCollision(Leaf leaf)
    {
        List<T> objs = new List<T>(CheckCollision(leaf.position, leaf.radius));
        objs.Remove(leaf.obj);
        return objs.ToArray();
    }
    public T[] CheckCollision(Vector2 checkPoint, float checkRadius)
    {
        return _root.DoCheckCollision(checkPoint, checkRadius);
    }
    T[] DoCheckCollision(Vector2 checkPoint, float checkRadius)
    {
        if (DontHaveChildren())
            return GetCollisionObjectsFromSelf(checkPoint, checkRadius);
        else
            return GetCollisionObjectsFromChildren(checkPoint, checkRadius);
    }

    T[] GetCollisionObjectsFromSelf(Vector2 checkPoint, float checkRadius)
    {
        List<T> objs = new List<T>();

        foreach (Leaf leaf in _leafs)
            if (Vector2.Distance(checkPoint, leaf.position) <= checkRadius + leaf.radius)
                objs.Add(leaf.obj);

        return objs.ToArray();
    }

    private T[] GetCollisionObjectsFromChildren(Vector2 checkPoint, float checkRadius)
    {
        List<T> objs = new List<T>();

        objs.AddRange(GetCollisionObjectsFromAChild(checkPoint, checkRadius, _upperRightChild));
        objs.AddRange(GetCollisionObjectsFromAChild(checkPoint, checkRadius, _lowerRightChild));
        objs.AddRange(GetCollisionObjectsFromAChild(checkPoint, checkRadius, _lowerLeftChild));
        objs.AddRange(GetCollisionObjectsFromAChild(checkPoint, checkRadius, _upperLeftChild));

        return objs.ToArray();
    }
    T[] GetCollisionObjectsFromAChild(Vector2 checkPoint, float checkRadius, QuadtreeData<T> child)
    {
        if (child._field.PointToFieldDistance(checkPoint) <= _maxRadius + checkRadius)      //这里不光要考虑到检测半径，还要考虑到节点最大半径
            return child.DoCheckCollision(checkPoint, checkRadius);
        return new T[] { };
    }



    //移除
    public bool RemoveLeaf(Leaf leaf)
    {
        return _root.DoRemoveLeaf(leaf);
    }
    bool DoRemoveLeaf(Leaf leaf)
    {
        if (DontHaveChildren())
            return RemoveLeafFromSelf(leaf);
        else
            return RemoveLeafFromChildren(leaf);
    }
    bool RemoveLeafFromSelf(Leaf leaf)
    {
        if (DoRemoveLeafFromSelf(leaf))
            return true;
        return _root.RemoveLeafInTotalTree(leaf);
    }
    private bool DoRemoveLeafFromSelf(Leaf leaf)
    {
        if (_leafs.Remove(leaf))
        {
            UpdateMaxRadiusWhenRemoveLeaf();
            return true;
        }
        return false;
    }
    void UpdateMaxRadiusWhenRemoveLeaf()
    {
        float newMaxRadius = GetLeafsMaxRadiusOnRemoveLeaf();
        if (_maxRadius != newMaxRadius)
        {
            _maxRadius = newMaxRadius;
            CallParentUpdateMaxRadius();
        }
    }
    float GetLeafsMaxRadiusOnRemoveLeaf()
    {
        float newMaxRadius = Mathf.NegativeInfinity;

        foreach (Leaf leaf in _leafs)
            if (leaf.radius > newMaxRadius)
                if (leaf.radius == _maxRadius)
                    return _maxRadius;
                else
                    newMaxRadius = leaf.radius;

        return newMaxRadius;
    }

    bool RemoveLeafFromChildren(Leaf leaf)
    {
        if (_upperRightChild._field.Contains(leaf.position))
            return _upperRightChild.DoRemoveLeaf(leaf);
        if (_lowerRightChild._field.Contains(leaf.position))
            return _lowerRightChild.DoRemoveLeaf(leaf);
        if (_lowerLeftChild._field.Contains(leaf.position))
            return _lowerLeftChild.DoRemoveLeaf(leaf);
        if (_upperLeftChild._field.Contains(leaf.position))
            return _upperLeftChild.DoRemoveLeaf(leaf);
        return _root.RemoveLeafInTotalTree(leaf);
    }



    bool RemoveLeafInTotalTree(Leaf leaf)
    {
        if (DontHaveChildren())
            return DoRemoveLeafFromSelf(leaf);
        else
            return RemoveLeafInTotalTreeFromChildren(leaf);
    }

    bool RemoveLeafInTotalTreeFromChildren(Leaf leaf)
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




    //从这开始是Debug代码，删掉不影响功能
    //绘制四叉树节点的范围
    void DrawField()
    {
        Vector3 upperRight = new Vector3(_field.right, _field.top, 0);
        Vector3 lowerRight = new Vector3(_field.right, _field.bottom, 0);
        Vector3 lowerLeft = new Vector3(_field.left, _field.bottom, 0);
        Vector3 upperLeft = new Vector3(_field.left, _field.top, 0);

        Debug.DrawLine(upperRight, lowerRight, Color.blue * 0.8f, Mathf.Infinity);
        Debug.DrawLine(lowerRight, lowerLeft, Color.blue * 0.8f, Mathf.Infinity);
        Debug.DrawLine(lowerLeft, upperLeft, Color.blue * 0.8f, Mathf.Infinity);
        Debug.DrawLine(upperLeft, upperRight, Color.blue * 0.8f, Mathf.Infinity);
    }
}