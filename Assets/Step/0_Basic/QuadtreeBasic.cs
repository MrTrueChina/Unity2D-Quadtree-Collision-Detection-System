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
 *  
 *  这个脚本里的是最基础的四叉树，由节点和叶子两部分构成。
 *  节点是四叉树的每个分支，节点组成树本身。
 *  叶子是碰撞器在树立的映射，树通过叶子判断是否发生碰撞。
 *  
 *  这个四叉树的功能只有插入、检测、移除，碰撞器没有半径是一个个点，并且如果碰撞器移动检测结果不会随之变化
 *  
 *  只有这个脚本是不能使用的，因此还有两个脚本是 QuadtreeBasicCollider 和 QuadtreeBasicObject
 *  QuadtreeBasicCollider 是碰撞器，挂载在物体上
 *  QuadtreeBasicObject   是四叉树物体，用来在场景加载时建立四叉树，之后才能进行碰撞检测
 */

using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.Quadtree.Step.Basic
{
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
        T _obj; //叶子映射的对象，在检测碰撞的时候会以数组形式返回出去

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
        QuadtreeBasicField _field; //这个类在下面

        QuadtreeBasic<T> _upperRightChild; //四个子节点
        QuadtreeBasic<T> _lowerRightChild;
        QuadtreeBasic<T> _lowerLeftChild;
        QuadtreeBasic<T> _upperLeftChild;

        List<QuadtreeBasicLeaf<T>> _leafs = new List<QuadtreeBasicLeaf<T>>();   //叶子List，用来存储这个节点里的叶子

        int _maxLeafsNumber; //这个节点里最多能容纳多少叶子，如果超过了这个值则要分割节点
        float _minSideLength; //可以分割到的最小边长，如果节点的宽或高已经小于这个值，那么不管有多少叶子都不能分割
        /*
         *  最小边长的设计是应对一种极端情况的：假设有大量的碰撞器他们的位置完全一样，那么无论怎么分割节点都不会把他们分隔开，分割将会无限循环下去
         *  增加最小边长之后就可以应对这种情况：就算是最极端情况也只会一口气分割到最小大小，不会无限分割下去
         */


        public QuadtreeBasic(float top, float right, float bottom, float left, int maxLeafsNumber, float minSideLength)
        {
            _field = new QuadtreeBasicField(top, right, bottom, left);

            _maxLeafsNumber = maxLeafsNumber;
            _minSideLength = minSideLength;

            DrawField(); //在 Scene 面板绘制四叉树范围，删掉不影响功能
        }

        /*
         *  存入叶子
         * 
         *  首先来解决一个用词混乱的问题：
         *      在二叉树里没有子节点的节点称为“叶子节点”或“叶子”，因为他们处于树的末端，在树的结构看来就是叶子一样。这样命名后就得到了子节点的节点是“树枝”、没有子节点的节点是“叶子”的形象命名。
         *      但在四叉树里这个命名方式行不通————四叉树里的“叶子”指的是联系碰撞器和树之间的映射对象，是和节点完全不同的类。
         *      因此我们将四叉树里有子节点的节点称为“树枝”或“树干”他们只负责构成四叉树，不负责储存叶子；处于四叉树末端的没有子节点的节点称为“树梢”，他们负责存储叶子；“叶子”则是映射对象，这样就形成了“树干”连接“树梢”，“树梢”连接“叶子”的形象命名。
         *      而没有父节点的那个节点和二叉树一样称为“根”或“根节点”
         *  
         *  存入叶子的流程是：
         *      先判断这个节点有没有子节点，没有的话说明是树梢，直接存入叶子就行。
         *      如果这个节点有子节点，则说明这是树枝，要向下寻找可以存入这个叶子的子节点。
         *          寻找可以存入子节点的叶子的方法很简单：如果叶子的位置到在子节点的区域里，则说明可以存进去。
         *          如此向下递归知道树梢再存进去就可以了。
         */
        public void SetLeaf(QuadtreeBasicLeaf<T> leaf)
        {
            if (DontHaveChildren())
                SetLeafToSelf(leaf);
            else
                SetLeafToChildren(leaf);
        }
        bool DontHaveChildren()
        {
            return _upperRightChild == null || _lowerRightChild == null || _lowerLeftChild == null || _upperLeftChild == null; //四个子节点是一起创建的，原理上说一个不存在另外三个也不存在，但假设只有一个不存在插入的叶子又在这个位置就要出事了
        }

        void SetLeafToSelf(QuadtreeBasicLeaf<T> leaf)
        {
            _leafs.Add(leaf);
            CheckAndDoSplit();
            Debug.Log("位置在(" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + ")的树梢节点存入位置在" + leaf.position + "的叶子");
        }

        void CheckAndDoSplit()
        {
            if (_leafs.Count > _maxLeafsNumber && _field.width > _minSideLength && _field.height > _minSideLength)
                Split();
        }

        void SetLeafToChildren(QuadtreeBasicLeaf<T> leaf)
        {
            Debug.Log("位置在(" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + ")的树枝节点向子节点存入位置在" + leaf.position + "的叶子");
            /*
             *  如果叶子在子节点的范围里，向这个子节点里存入叶子
             *  用 else if 的原因是 Field.Contains 把边缘处的点也算在范围里，这如果一个叶子在两个节点的交界处只用一个 if 就会重复存入
             */
            if (_upperRightChild._field.Contains(leaf.position))
                _upperRightChild.SetLeaf(leaf);
            else if (_lowerRightChild._field.Contains(leaf.position))
                _lowerRightChild.SetLeaf(leaf);
            else if (_lowerLeftChild._field.Contains(leaf.position))
                _lowerLeftChild.SetLeaf(leaf);
            else if (_upperLeftChild._field.Contains(leaf.position))
                _upperLeftChild.SetLeaf(leaf);
        }

        /*
         *  分割，四叉树的核心来了
         *  
         *  分割就是先把这个节点的范围分成四份，分别给四个子节点，之后把这个节点里的所有叶子按位置存进四个子节点里。
         *  分割完成后这个节点就从树梢变成了树枝，而四个子节点成为了新的树梢。
         */
        void Split()
        {
            Debug.Log("位置在(" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + ")的树梢节点达到分割条件，进行分割");

            //计算出横竖的中心坐标
            float xCenter = (_field.left + _field.right) / 2;
            float yCenter = (_field.bottom + _field.top) / 2;

            //用上面计算的中心坐标和原本的区域组合出四个子节点的区域来，并且把最大叶子数、最小宽高一起传给子节点
            _upperRightChild = new QuadtreeBasic<T>(_field.top, _field.right, yCenter, xCenter, _maxLeafsNumber, _minSideLength);
            _lowerRightChild = new QuadtreeBasic<T>(yCenter, _field.right, _field.bottom, xCenter, _maxLeafsNumber, _minSideLength);
            _lowerLeftChild = new QuadtreeBasic<T>(yCenter, xCenter, _field.bottom, _field.left, _maxLeafsNumber, _minSideLength);
            _upperLeftChild = new QuadtreeBasic<T>(_field.top, xCenter, yCenter, _field.left, _maxLeafsNumber, _minSideLength);

            //生成完子节点后把这个节点里的所有叶子分给子节点
            foreach (QuadtreeBasicLeaf<T> leaf in _leafs) //假设你不会用 foreach ，请看 QuadtreeBasicDetector
                SetLeafToChildren(leaf);
            _leafs = null; //将叶子分给子节点后这个节点就不需要继续保留叶子了，把叶子List设为null，让C#的清理器来清理掉节省内存
        }

        /*
         *  碰撞检测
         *  
         *  四叉树的碰撞检测同样分两部分：
         *  树梢部分非常无脑：暴力检测全部叶子是否碰撞
         *  树枝部分则十分巧妙，需要一些解说：
         *      首先根据四叉树的设计，每个节点都有一个区域，不论是树枝还是树梢。
         *      树梢里存在着数量不固定的叶子，可以分布在树梢范围内的任何一个位置。
         *      树枝本身虽然不保存叶子，但树枝的区域和他的子节点树梢的范围是重合的，也就是说在树枝的范围里同样有数量不固定的叶子，可能分布在任意一个位置。
         *      再看四叉树碰撞检测的流程： 找出可能有叶子发生碰撞的节点 -> 在这个节点的四个子节点里找出可能发生碰撞的节点 -> 在这个节点的四个子节点里找出可能发生碰撞的节点 -> ... -> 找到可能有叶子发生碰撞的树梢节点 -> 暴力检测这个树梢下面的叶子
         *      
         *      这样可以得出一个结论：如果检测的范围与节点的范围有重合，那么这个节点下面就有可能有叶子会发生碰撞。
         *      又因为检测范围是一个圆，所以通过比较检测点到节点区域的距离和检测半径就能判断出这个节点是否可能发生碰撞。
         */
        public T[] CheckCollision(Vector2 checkPosition, float checkRadius)
        {
            if (DontHaveChildren())
                return GetCollisionObjectsFromSelf(checkPosition, checkRadius);
            else
                return GetCollisionObjectsFromChildren(checkPosition, checkRadius);
        }

        T[] GetCollisionObjectsFromSelf(Vector2 checkPosition, float checkRadius)
        {
            List<T> objs = new List<T>();

            foreach (QuadtreeBasicLeaf<T> leaf in _leafs)
                if (Vector2.Distance(checkPosition, leaf.position) <= checkRadius) //开头说的没有一丁点技术含量的距离检测， Vector2.Distance可以计算出两个点的距离，之后和检测半径一比较就知道有没有检测到了
                    objs.Add(leaf.obj);

            return objs.ToArray();
        }

        T[] GetCollisionObjectsFromChildren(Vector2 checkPosition, float checkRadius)
        {
            List<T> objs = new List<T>();

            if (_upperRightChild._field.PointToFieldDistance(checkPosition) <= checkRadius)
                objs.AddRange(_upperRightChild.CheckCollision(checkPosition, checkRadius)); //这里用的不是 if else 而是连续存入，因为检测的范围是一个圆，基本不可能只在一个节点范围里，因此要把每个子节点都考虑到
            if (_lowerRightChild._field.PointToFieldDistance(checkPosition) <= checkRadius)
                objs.AddRange(_lowerRightChild.CheckCollision(checkPosition, checkRadius));
            if (_lowerLeftChild._field.PointToFieldDistance(checkPosition) <= checkRadius)
                objs.AddRange(_lowerLeftChild.CheckCollision(checkPosition, checkRadius));
            if (_upperLeftChild._field.PointToFieldDistance(checkPosition) <= checkRadius)
                objs.AddRange(_upperLeftChild.CheckCollision(checkPosition, checkRadius));

            return objs.ToArray();
        }

        /*
         *  移除叶子
         *  
         *  移除叶子很简单，先是跟插入叶子时一样根据坐标找到叶子所在的树梢，之后直接移除就可以了。
         */
        public void RemoveLeaf(QuadtreeBasicLeaf<T> leaf)
        {
            if (DontHaveChildren())
                RemoveLeafFromSelf(leaf);
            else
                RemoveLeafFromChildren(leaf);
        }

        void RemoveLeafFromSelf(QuadtreeBasicLeaf<T> leaf)
        {
            _leafs.Remove(leaf);

            Debug.Log("位置在(" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + ")的树梢节点移除位置在" + leaf.position + "的叶子");
        }

        void RemoveLeafFromChildren(QuadtreeBasicLeaf<T> leaf)
        {
            Debug.Log("位置在(" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + ")的树枝节点从子节点移除位置在" + leaf.position + "的叶子");

            if (_upperRightChild._field.Contains(leaf.position))
                _upperRightChild.RemoveLeaf(leaf);
            if (_lowerRightChild._field.Contains(leaf.position))
                _lowerRightChild.RemoveLeaf(leaf);
            if (_lowerLeftChild._field.Contains(leaf.position))
                _lowerLeftChild.RemoveLeaf(leaf);
            if (_upperLeftChild._field.Contains(leaf.position))
                _upperLeftChild.RemoveLeaf(leaf);
        }

        //从这开始是Debug代码，删掉不影响功能
        //绘制四叉树节点的范围
        void DrawField()
        {
            Vector3 upperRight = new Vector3(_field.right, _field.top, 0);
            Vector3 lowerRight = new Vector3(_field.right, _field.bottom, 0);
            Vector3 lowerLeft = new Vector3(_field.left, _field.bottom, 0);
            Vector3 upperLeft = new Vector3(_field.left, _field.top, 0);

            Debug.DrawLine(upperRight, lowerRight, Color.blue * 0.8f, Mathf.Infinity); //Mathf.Infinity：正无穷，显示时间设置为正无穷就会一直显示
            Debug.DrawLine(lowerRight, lowerLeft, Color.blue * 0.8f, Mathf.Infinity); //Color：颜色类，Color.blue：自带的蓝色
            Debug.DrawLine(lowerLeft, upperLeft, Color.blue * 0.8f, Mathf.Infinity);
            Debug.DrawLine(upperLeft, upperRight, Color.blue * 0.8f, Mathf.Infinity);
            //Debug.DrawLine(Vector3 start, Vector3 end, Color color, float duration)：画线，参数依次是：起点、终点、颜色、显示时间
            //【注意】这个方法的持续时间是基于 Time.deltaTime 实现的，也就是说只有在游戏运行模式下才有效，如果在编辑模式下使用，绘制的线不管经过多长时间都不会消失
        }
    }

    public class QuadtreeBasicField
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

        public QuadtreeBasicField(float top, float right, float bottom, float left)
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
            float xDistance = Mathf.Max(0, point.x - _right, _left - point.x); //这一步是这样的：如果点在左边，则左边坐标 - 点是正数，返回距离；如果在右边，则点 - 右边坐标是正数，返回距离；如果在中间，则两个计算都是负数，返回0
            float yDistance = Mathf.Max(0, point.y - _top, _bottom - point.y);
            return Mathf.Sqrt(xDistance * xDistance + yDistance * yDistance); //三角函数，别说这个你不会
        }
    }
}
