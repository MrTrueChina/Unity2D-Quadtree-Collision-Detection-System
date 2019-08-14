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

using System.Collections.Generic;
using UnityEngine;


public class QuadtreeWithUpdateLeaf<T>
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


    public QuadtreeWithUpdateLeaf(T obj, Vector2 position, float radius)
    {
        _obj = obj;
        _position = position;
        _radius = radius;
    }
}


public class QuadtreeWithUpdate<T>
{
    QuadtreeWithUpdateField _field;

    float _maxRadius = Mathf.NegativeInfinity;

    QuadtreeWithUpdate<T> _root;
    /*
     *  根节点，前面已经说到更新位置时需要从根节点再次存入叶子。
     *  虽然可以通过向上递归父节点查到根节点但找个字段存下来计算量小。
     */
    
    QuadtreeWithUpdate<T> _parent;
    QuadtreeWithUpdate<T> _upperRightChild;
    QuadtreeWithUpdate<T> _lowerRightChild;
    QuadtreeWithUpdate<T> _lowerLeftChild;
    QuadtreeWithUpdate<T> _upperLeftChild;

    List<QuadtreeWithUpdateLeaf<T>> _leafs = new List<QuadtreeWithUpdateLeaf<T>>();

    int _maxLeafsNumber;
    float _minSideLength;



    //构造，构造方法增加根节点参数，如果不传参则设为自身（只有创建根节点时不传参）
    public QuadtreeWithUpdate(float top, float right, float bottom, float left, int maxLeafNumber, float minSideLength, QuadtreeWithUpdate<T> root = null, QuadtreeWithUpdate<T> parent = null)
    {
        _field = new QuadtreeWithUpdateField(top, right, bottom, left);

        _maxLeafsNumber = maxLeafNumber;
        _minSideLength = minSideLength;

        _root = root != null ? root : this;
        /*
         *   ? : 运算符，英文冒号(:)是运算符的一部分。
         *  ? :运算符是唯一的三参数运算符（暂时的，但几十年都没有出第二个），所以很多人直接叫他“三目运算符”
         *  他的格式是这样的：
         *  条件 ? 参数1 : 参数2
         *  如果条件为真，返回参数1，条件为假，返回参数2。
         *  
         *  特别方便，本来要if else四行的代码只要一行就搞定了
         */

        _parent = parent;
    }



    /*
     *  存入增加存入叶子不在整个四叉树范围内的情况的判断
     *  
     *  这个判断在最开始就应该写上，但在前面并不会造成特别严重的后果，为了降低知识密度就没写，但在增加了更新之后，一旦有一个叶子的位置不在整个树的范围内，马上就会导致死循环
     */
    public bool SetLeaf(QuadtreeWithUpdateLeaf<T> leaf)
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

    bool SetLeafToSelf(QuadtreeWithUpdateLeaf<T> leaf)
    {
        if (this == _root && !_field.Contains(leaf.position))
        {
            Debug.LogError("存入叶子失败，叶子不在四叉树范围内");
            return false;
        }

        _leafs.Add(leaf);
        UpdateMaxRadiusWhenSetLeaf(leaf);
        Debug.Log("位置在" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + "的树梢节点存入位置在" + leaf.position + "半径是" + leaf.radius + "的叶子，存入后的最大半径是" + _maxRadius);
        CheckAndDoSplit();
        return true;
    }
    void UpdateMaxRadiusWhenSetLeaf(QuadtreeWithUpdateLeaf<T> leaf)
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
            Debug.Log("位置在" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + "的树枝节点更新最大半径，更新后的最大半径是" + _maxRadius);
            CallParentUpdateMaxRadius();
        }
    }
    float GetChildrenMaxRadius()
    {
        return Mathf.Max(_upperRightChild._maxRadius, _lowerRightChild._maxRadius, _lowerLeftChild._maxRadius, _upperLeftChild._maxRadius);
    }

    bool SetLeafToChildren(QuadtreeWithUpdateLeaf<T> leaf)
    {
        Debug.Log("位置在" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + "的树枝节点向子节点存入位置在" + leaf.position + "半径是" + leaf.radius + "的叶子");

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



    /*
     *  【非常重要】在分割之前先进行一次更新，从这一步开始叶子可以移动，因此有可能在分割的时候有的叶子已经移出了这个树梢的范围，那么在分割完后重新存入叶子的过程中就会出现叶子不在所有子节点范围里的bug，为此在分割之前先进行一次更新
     *  
     *  次要：在分割子节点时存入根节点，便于重新存入叶子
     */
    void CheckAndDoSplit()
    {
        if (_leafs.Count > _maxLeafsNumber && _field.width > _minSideLength && _field.height > _minSideLength)
            Split();
    }
    void Split()
    {
        Debug.Log("位置在" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + "的树梢节点达到分割条件，进行分割");

        Update();       //这一步非常重要！隐形巨坑！有时候有的叶子已经脱离了他所在的树梢的范围但还没有更新，在分割完成后重新存入叶子的时候就会发生错误，在分割前先进行一次更新，因为只有自己的叶子要重新存入，所以自己调用更新自己就行，不需要根节点进行全部更新

        float xCenter = (_field.left + _field.right) / 2;
        float yCenter = (_field.bottom + _field.top) / 2;

        _upperRightChild = new QuadtreeWithUpdate<T>(_field.top, _field.right, yCenter, xCenter, _maxLeafsNumber, _minSideLength, _root, this);
        _lowerRightChild = new QuadtreeWithUpdate<T>(yCenter, _field.right, _field.bottom, xCenter, _maxLeafsNumber, _minSideLength, _root, this);
        _lowerLeftChild = new QuadtreeWithUpdate<T>(yCenter, xCenter, _field.bottom, _field.left, _maxLeafsNumber, _minSideLength, _root, this);
        _upperLeftChild = new QuadtreeWithUpdate<T>(_field.top, xCenter, yCenter, _field.left, _maxLeafsNumber, _minSideLength, _root, this);

        foreach (QuadtreeWithUpdateLeaf<T> leaf in _leafs)
            SetLeafToChildren(leaf);
        _leafs = null;
    }



    //更新
    public void Update()
    {
        UpdatePosition();
        UpdateMaxRadius();

        DrawField();    //绘制节点范围，删除不影响功能，移到更新是因为之前用无限时长绘制的线在场景切换后还是存在，但场景切换时四叉树已经销毁了
    }

    void UpdatePosition()
    {
        if (DontHaveChildren())
            UpdatePositionSelf();
        else
            UpdateChildrensPosition();
    }
    void UpdatePositionSelf()
    {
        List<QuadtreeWithUpdateLeaf<T>> resetLeafs = new List<QuadtreeWithUpdateLeaf<T>>();
        /*
         *  需要重新存入的叶子List，如果检测的同时进行重新存入有可能会导致有的叶子检测好几次，有的叶子没检测到。
         *  先遍历一次找到需要重新存入的叶子，之后再一起重新存入。
         */

        foreach (QuadtreeWithUpdateLeaf<T> leaf in _leafs)
            if (!_field.Contains(leaf.position))
                resetLeafs.Add(leaf);

        foreach (QuadtreeWithUpdateLeaf<T> leaf in resetLeafs)
            ResetLeaf(leaf);
    }
    void ResetLeaf(QuadtreeWithUpdateLeaf<T> leaf)
    {
        Debug.Log("位置在" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + "的树梢节点移除位置在" + leaf.position + "半径是" + leaf.radius + "的叶子，重新存入树");
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
            UpdateMaxRadiusSelf();
        else
            UpdateChildrensMaxRadius();
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

        foreach (QuadtreeWithUpdateLeaf<T> leaf in _leafs)
            if (leaf.radius > newMaxRadius)
                newMaxRadius = leaf.radius;

        return newMaxRadius;
        /*
         *  看起来和移除叶子时候的那个获取最大叶子最大半径的方法很像。
         *  但因为碰撞器的半径可能会增大，所以不能遇到现在的最大半径就返回。
         */
    }
    void UpdateChildrensMaxRadius()
    {
        _upperRightChild.UpdateMaxRadius();
        _lowerRightChild.UpdateMaxRadius();
        _lowerLeftChild.UpdateMaxRadius();
        _upperLeftChild.UpdateMaxRadius();
    }



    //检测
    public T[] CheckCollision(Vector2 checkPoint, float checkRadius)
    {
        if (DontHaveChildren())
            return GetCollisionObjectsFromSelf(checkPoint, checkRadius);
        else
            return GetCollisionObjectsFromChildren(checkPoint, checkRadius);
    }

    T[] GetCollisionObjectsFromSelf(Vector2 checkPoint, float checkRadius)
    {
        List<T> objs = new List<T>();

        foreach (QuadtreeWithUpdateLeaf<T> leaf in _leafs)
            if (Vector2.Distance(checkPoint, leaf.position) <= checkRadius + leaf.radius)
                objs.Add(leaf.obj);

        return objs.ToArray();
    }

    T[] GetCollisionObjectsFromChildren(Vector2 checkPoint, float checkRadius)
    {
        List<T> objs = new List<T>();

        objs.AddRange(GetCollisionObjectsFromAChild(checkPoint, checkRadius, _upperRightChild));
        objs.AddRange(GetCollisionObjectsFromAChild(checkPoint, checkRadius, _lowerRightChild));
        objs.AddRange(GetCollisionObjectsFromAChild(checkPoint, checkRadius, _lowerLeftChild));
        objs.AddRange(GetCollisionObjectsFromAChild(checkPoint, checkRadius, _upperLeftChild));

        return objs.ToArray();
    }
    T[] GetCollisionObjectsFromAChild(Vector2 checkPoint, float checkRadius, QuadtreeWithUpdate<T> child)
    {
        if (child._field.PointToFieldDistance(checkPoint) <= _maxRadius + checkRadius)      //这里不光要考虑到检测半径，还要考虑到节点最大半径
            return child.CheckCollision(checkPoint, checkRadius);
        return new T[0];
    }



    //移除，增加了全树移除
    public bool RemoveLeaf(QuadtreeWithUpdateLeaf<T> leaf)
    {
        if (DontHaveChildren())
            return RemoveLeafFromSelf(leaf);
        else
            return RemoveLeafFromChildren(leaf);
    }

    bool RemoveLeafFromSelf(QuadtreeWithUpdateLeaf<T> leaf)
    {
        if (DoRemoveLeafFromSelf(leaf))
            return true;
        return _root.RemoveLeafInTotalTree(leaf);
    }
    bool DoRemoveLeafFromSelf(QuadtreeWithUpdateLeaf<T> leaf)
    {
        if (_leafs.Remove(leaf))        //List的Remove返回有没有成功从List里移除要移除的元素，元素不存在的时候返回是 false，有了这个返回值就可以非常轻松的判断出这个树梢是不是成功移除了叶子
        {
            UpdateMaxRadiusWhenRemoveLeaf();
            Debug.Log("位置在" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + "的树梢节点移除位置在" + leaf.position + "半径是" + leaf.radius + "的叶子，移除后的最大半径是" + _maxRadius);
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
            Debug.Log("位置在" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + "的树梢节点半径发生变化，新半径是" + _maxRadius);
            CallParentUpdateMaxRadius();
        }
    }
    float GetLeafsMaxRadiusOnRemoveLeaf()
    {
        float newMaxRadius = Mathf.NegativeInfinity;

        foreach (QuadtreeWithUpdateLeaf<T> leaf in _leafs)
            if (leaf.radius > newMaxRadius)
                if (leaf.radius == _maxRadius)
                    return _maxRadius;
                else
                    newMaxRadius = leaf.radius;

        return newMaxRadius;
    }

    bool RemoveLeafFromChildren(QuadtreeWithUpdateLeaf<T> leaf)
    {
        Debug.Log("位置在(" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + ")的树枝节点从子节点移除位置在" + leaf.position + "半径是" + leaf.radius + "的叶子");
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



    /*
     *  全树移除，不通过位置判断而是直接遍历整棵树移除叶子
     *  
     *  实际上在上一步就应该增加全树移除，但考虑到上一步没有实用性为了降低知识密度就没加。
     *  
     *  前面的移除都是根据叶子位置找到树梢之后进行删除，但这个寻找方式是有缺陷的，在几种情况下都会失效
     *  1.叶子移出了树梢范围，叶子不在原本的树梢上按照位置当然找不到正确的树梢，不要以这种情况为有了更新就不会发生，更新每帧只进行一次，叶子的移动则是不限量的
     *  2.叶子移出了整棵树的范围，情况更严重，从根节点开始就不可能找得到叶子所在的树梢
     *  3.叶子根本不在树里，几乎找茬的情况，无论在哪个树梢都不可能找到这个叶子，更不可能移除掉
     *  
     *  如果是情况1，可以通过更新树或全树移除来解决，但对于2和3更新无异于自寻死路，这样唯一的解就是不通过位置寻找树梢而是遍历整棵树找到这个叶子删掉他
     */
    bool RemoveLeafInTotalTree(QuadtreeWithUpdateLeaf<T> leaf)
    {
        if (DontHaveChildren())
            return DoRemoveLeafFromSelf(leaf);
        else
            return RemoveLeafInTotalTreeFromChildren(leaf);
    }

    bool RemoveLeafInTotalTreeFromChildren(QuadtreeWithUpdateLeaf<T> leaf)
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
        if (DontHaveChildren())
        {
            Vector3 upperRight = new Vector3(_field.right, _field.top, 0);
            Vector3 lowerRight = new Vector3(_field.right, _field.bottom, 0);
            Vector3 lowerLeft = new Vector3(_field.left, _field.bottom, 0);
            Vector3 upperLeft = new Vector3(_field.left, _field.top, 0);

            Debug.DrawLine(upperRight, lowerRight, Color.blue * 0.8f, 0);
            Debug.DrawLine(lowerRight, lowerLeft, Color.blue * 0.8f, 0);
            Debug.DrawLine(lowerLeft, upperLeft, Color.blue * 0.8f, 0);
            Debug.DrawLine(upperLeft, upperRight, Color.blue * 0.8f, 0);
        }
        else
        {
            _upperRightChild.DrawField();
            _lowerRightChild.DrawField();
            _lowerLeftChild.DrawField();
            _upperLeftChild.DrawField();
        }
    }
}



public class QuadtreeWithUpdateField
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



    public QuadtreeWithUpdateField(float top, float right, float bottom, float left)
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