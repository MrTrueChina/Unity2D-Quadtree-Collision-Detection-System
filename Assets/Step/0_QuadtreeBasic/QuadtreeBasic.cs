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
 *  只有这个脚本是不能使用的，因此还有两个脚本是 QuadtreeBasicCollider 和 QuadtreeBasicObject
 *  QuadtreeBasicCollider 是碰撞器，挂载在物体上
 *  QuadtreeBasicObject   是四叉树物体，用来在场景加载时建立四叉树，之后才能进行碰撞检测
 */

using System.Collections.Generic;
using UnityEngine;

/*
 *  这个<T>是“泛型”，有泛型的类和方法可以在调用的时候指定类型，Unity的 GetComponent<> 就应用了泛型
 */
public class QuadtreeBasicLeaf<T>
{
    /*
     *  get,set访问器，C#的常用封装方式，看起来是这样的：
     *  
     *  修饰符 类型 访问器名
     *  {
     *      get
     *      {
     *          这里面需要return
     *      }
     *      set
     *      {
     *          这里存入的量用 value 表示
     *      }
     *  }
     *  
     *  假设有一个访问器是这样的：
     *  public int age
     *  {
     *      get
     *      {
     *          return _age;
     *      }
     *      set
     *      {
     *          _age = value;
     *      }
     *  }
     *  int _age
     *  
     *  赋值会调用 set 进行赋值，赋值在 set 里就是那个 value
     *  取值会调用 get 进行返回
     */
    public T obj
    {
        get { return _obj; }
    }
    T _obj;         //叶子映射的对象，在检测碰撞的时候会以数组形式返回出去

    public Vector2 position
    {
        get { return _position; }
        set { _position = value; }
    }
    Vector2 _position;


    public QuadtreeBasicLeaf(T obj, Vector2 position)
    {
        _obj = obj;
        _position = position;
    }
}

public class QuadtreeBasic<T>
{
    Rect _rect;                             //Rect是Unity自带的类，用来表示矩形，用这个矩形代表这个节点的空间
    
    QuadtreeBasic<T> _upperRightChild;      //四个子节点
    QuadtreeBasic<T> _lowerRightChild;
    QuadtreeBasic<T> _lowerLeftChild;
    QuadtreeBasic<T> _upperLeftChild;

    List<QuadtreeBasicLeaf<T>> _leafs = new List<QuadtreeBasicLeaf<T>>();   //叶子List，用来存储这个节点里的叶子

    int _maxLeafsNomber;                    //这个节点里最多能容纳多少叶子，如果超过了这个值则要分割节点
    float _minWidth;                        //可以分割到的最小宽度，如果节点的宽度已经小于这个值，那么不管有多少叶子都不能分割
    float _minHeight;                       //最小高度，与最小宽度相同的作用
    /*
     *  最小宽高的设计是应对一种极端情况的：假设有大量的碰撞器他们的位置完全一样，那么无论怎么分割节点都不会把他们分隔开，分割将会无限循环下去
     *  增加最小宽高之后就可以应对这种情况：就算是最极端情况也只会一口气分割到最小大小，不会无线分割下去
     */


    /*
     *  参数默认值，就是在参数后面写 = 值，有默认值的参数必须在没有默认值的参数后面，因为在前面的话编译器无法分辨调用时是传给有默认值的参数还是传给没默认值的参数
     */
    public QuadtreeBasic(float x, float y, float width, float height, int splitLeafsNumber = 10, float minWidth = 1, float minHeight = 1)
    {
        _rect = new Rect(x, y, width, height);          //Rect的一种构造方法，四个参数依次是：x坐标，y坐标，宽度，高度。x,y两个参数不是中心坐标，是在角上的

        _maxLeafsNomber = splitLeafsNumber;
        _minWidth = minWidth;
        _minHeight = minHeight;
    }


    //存入叶子，原理和二叉树一样，通过递归一路向下找到正确的节点，把叶子存进那个节点里
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
        CheckAndDoSplit();
    }
    void CheckAndDoSplit()
    {
        if (_leafs.Count > _maxLeafsNomber && _rect.width > _minWidth && _rect.height > _minHeight)
            Split();
    }

    void SetLeafToChildren(QuadtreeBasicLeaf<T> leaf)
    {
        /*
         *  计算存入的叶子的位置和子节点空间的距离，距离为0则说明在子节点范围里，向这个子节点里存入叶子
         *  用 else if 的原因是计算距离的方法把处在矩形边缘的点距离算成0，处于两个子节点交界处的叶子在多个子节点的范围里，只用 if 会导致一个叶子存进多个节点里
         *  
         *  PointToRectDistance 是一个扩展方法，位置在 Quadtree 里，用处是计算一个点到矩形的距离，如果点在矩形内部则是0
         */
        if (_upperRightChild._rect.PointToRectDistance(leaf.position) == 0)
            _upperRightChild.SetLeaf(leaf);
        else if (_lowerRightChild._rect.PointToRectDistance(leaf.position) == 0)
            _lowerRightChild.SetLeaf(leaf);
        else if (_lowerLeftChild._rect.PointToRectDistance(leaf.position) == 0)
            _lowerLeftChild.SetLeaf(leaf);
        else if (_upperLeftChild._rect.PointToRectDistance(leaf.position) == 0)
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

        _upperRightChild = new QuadtreeBasic<T>(rightX, upperY, childWidth, childHeight, _maxLeafsNomber, _minWidth, _minHeight);
        _lowerRightChild = new QuadtreeBasic<T>(rightX, _rect.y, childWidth, childHeight, _maxLeafsNomber, _minWidth, _minHeight);
        _lowerLeftChild = new QuadtreeBasic<T>(_rect.x, _rect.y, childWidth, childHeight, _maxLeafsNomber, _minWidth, _minHeight);
        _upperLeftChild = new QuadtreeBasic<T>(_rect.x, upperY, childWidth, childHeight, _maxLeafsNomber, _minWidth, _minHeight);

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
            if (_upperRightChild._rect.PointToRectDistance(checkPosition) <= checkRadius)
                objs.AddRange(_upperRightChild.CheckCollision(checkPosition, checkRadius));
            if (_lowerRightChild._rect.PointToRectDistance(checkPosition) <= checkRadius)
                objs.AddRange(_lowerRightChild.CheckCollision(checkPosition, checkRadius));
            if (_lowerLeftChild._rect.PointToRectDistance(checkPosition) <= checkRadius)
                objs.AddRange(_lowerLeftChild.CheckCollision(checkPosition, checkRadius));
            if (_upperLeftChild._rect.PointToRectDistance(checkPosition) <= checkRadius)
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
            if (_upperRightChild._rect.PointToRectDistance(leaf.position) == 0)
                _upperRightChild.RemoveLeaf(leaf);
            if (_lowerRightChild._rect.PointToRectDistance(leaf.position) == 0)
                _lowerRightChild.RemoveLeaf(leaf);
            if (_lowerLeftChild._rect.PointToRectDistance(leaf.position) == 0)
                _lowerLeftChild.RemoveLeaf(leaf);
            if (_upperLeftChild._rect.PointToRectDistance(leaf.position) == 0)
                _upperLeftChild.RemoveLeaf(leaf);
        }
    }
}
