/*
 *  增加了半径的四叉树，同样有配合使用的 QuadtreeWithRadiusObject 和 QuadtreeWithRadiusCollider
 *  
 *  增加了半径后存入、检测、移除的步骤都有所改变
 *  
 *  先从检测开始：
 *      在第零步里面已经写过如何判断，原理是自上到下找到树梢之后计算距离和检测半径。
 *      在增加了碰撞器半径后向下找到树梢的步骤出现了变化：
 *          第零步的叶子没有半径，不管叶子在节点区域里的哪个位置，叶子的碰撞范围都不会超出节点范围。
 *          这一步的叶子有了半径，如果叶子的位置比较靠近节点区域边缘，这个叶子的碰撞范围就会超出节点区域。
 *          这样向下检测一个子节点有没有可能发生碰撞时就需要考虑到这个子节点区域里半径最大的那个节点可能已经处于边缘处。
 *          当然我们可以通过遍历节点里所有的叶子得出最大半径，但这样暴力解决计算量比不用四叉树还大。
 *          为了减少计算量我们可以给每个节点存一个最大半径，这样就只需要查看一次最大半径就可以判断出结果。
 *          也就是说需要增加最大半径。
 *      
 *      之后看存入：
 *          第零步光存入就算完，但增加了最大半径这个值后就不一样了，需要每次存入的时候都更新最大半径。
 *          光是自身更新还不够，还要向上更新，否则会出现这样一种情况：
 *              在左上角有一个树梢里有一个叶子半径巨大无比，碰撞区域比整个四叉树的区域还要大。
 *              测试点在右下角。
 *              在根节点向下找的时候法线左上方的子节点的最大半径很小，以为他不会发生碰撞，没有向下寻找。
 *              这样这个不管检测哪都会碰到的叶子就因为有一节数值的最大半径太小而被忽略掉了。
 *              为了应对这种情况必须要向上更新最大半径。
 *              
 *      最后是移除
 *          第零步的移除也是移除就算完了，这一步的移除则要计算新的最大半径。
 *          当然也要向上更新，不然最大的叶子已经没了还计算过来会浪费计算量。
 *      
 *  附带增加了存入和移除叶子的是否成功的bool返回
 */

using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.Quadtree.Example.Step1Radius
{
    public class QuadtreeLeafRadius<T>
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

        public QuadtreeLeafRadius(T obj, Vector2 position, float radius)
        {
            _obj = obj;
            _position = position;
            _radius = radius;
        }
    }



    public class QuadtreeRadius<T>
    {
        QuadtreeWithRadiusField _field;

        float _maxRadius = Mathf.NegativeInfinity;
        /*
         *  这个值代表着这个节点里半径最大的那个叶子的半径，节点里没有叶子时设为0或者任何负数都不影响正确性，设为负无穷的原因如下：
         *  
         *  由于这个四叉树在第零步的基础上增加了叶子的半径，碰撞检测向下递归时判断子节点有没有可能有叶子发生碰撞就变得复杂了：
         *  一个节点里可以有好多种半径的叶子，每个叶子可能在任意位置，因此要判断一个子节点里有没有叶子可能发生碰撞的方法变成了 计算检测点到子节点区域的距离，如果这个距离小于测试半径和子节点最大半径的叶子的半径，则说明这个子节点里有可能有叶子会碰撞到。
         *  
         *  根据这个方法，如果一个子节点的最大半径是负数，则判断会发生有趣的现象：检测范围已经覆盖到了这个节点的范围，但因为节点半径是负数，判断上要减去这个值。
         *  那么如果一个子节点的最大半径是负无穷，那么即使测试半径再怎么大，这个节点也不会被判断为可能发生碰撞。（除非测试半径是正无穷，设为正无穷肯定不是正常流程，至少不是这个四叉树的正常流程）
         *  如果将没有叶子的节点的最大半径设为负无穷，则碰撞检测时这个节点判断为不会发生碰撞，就不会向下迭代这个节点，可以节省一点计算量。
         */

        QuadtreeRadius<T> _parent;
        /*
         *  父节点，用来向上更新最大半径
         */

        QuadtreeRadius<T> _upperRightChild;
        QuadtreeRadius<T> _lowerRightChild;
        QuadtreeRadius<T> _lowerLeftChild;
        QuadtreeRadius<T> _upperLeftChild;

        List<QuadtreeLeafRadius<T>> _leafs = new List<QuadtreeLeafRadius<T>>();

        int _maxLeafsNumber;
        float _minSideLength;

        /*
         *  构造方法增加一个父节点参数，默认值是null。
         *  创建四叉树时不传参数，分割时传当前节点，这样根节点就没有父节点，其他节点都有父节点，向上更新半径就可以进行。
         */
        public QuadtreeRadius(float top, float right, float bottom, float left, int maxLeafNumber, float minSideLength, QuadtreeRadius<T> parent = null)
        {
            _field = new QuadtreeWithRadiusField(top, right, bottom, left);

            _maxLeafsNumber = maxLeafNumber;
            _minSideLength = minSideLength;

            _parent = parent;

            DrawField();    //绘制节点范围，删除不影响功能
        }

        /*
         *  存入叶子
         *  在第零步的基础上增加了更新半径功能和返回存入是否成功功能
         */
        public bool SetLeaf(QuadtreeLeafRadius<T> leaf)
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

        bool SetLeafToSelf(QuadtreeLeafRadius<T> leaf)
        {
            _leafs.Add(leaf);
            UpdateMaxRadiusWhenSetLeaf(leaf);
            Debug.Log("位置在(" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + ")的树梢节点存入位置在" + leaf.position + "半径是" + leaf.radius + "的叶子，存入后的最大半径是" + _maxRadius);
            CheckAndDoSplit();
            return true;
        }

        void UpdateMaxRadiusWhenSetLeaf(QuadtreeLeafRadius<T> leaf)
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
            float newMaxRaiuds = GetChildrenMaxRadius();
            if (newMaxRaiuds != _maxRadius)
            {
                _maxRadius = newMaxRaiuds;
                Debug.Log("位置在(" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + ")的树枝节点更新最大半径，更新后的最大半径是" + _maxRadius);
                CallParentUpdateMaxRadius();
            }
        }

        float GetChildrenMaxRadius()
        {
            return Mathf.Max(_upperRightChild._maxRadius, _lowerRightChild._maxRadius, _lowerLeftChild._maxRadius, _upperLeftChild._maxRadius);
        }

        bool SetLeafToChildren(QuadtreeLeafRadius<T> leaf)
        {
            Debug.Log("位置在(" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + ")的树枝节点向子节点存入位置在" + leaf.position + "半径是" + leaf.radius + "的叶子");

            if (_upperRightChild._field.Contains(leaf.position))
                return _upperRightChild.SetLeaf(leaf);
            if (_lowerRightChild._field.Contains(leaf.position))
                return _lowerRightChild.SetLeaf(leaf);
            if (_lowerLeftChild._field.Contains(leaf.position))
                return _lowerLeftChild.SetLeaf(leaf);
            if (_upperLeftChild._field.Contains(leaf.position))
                return _upperLeftChild.SetLeaf(leaf);

            Debug.LogError("向位置在(" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + ")的节点存入叶子时发生错误：叶子不在所有子节点的范围里。");   //Debug.LogError：在Console面板输出Error，就是红色那种消息
            return false;
        }

        /*
         *  分割跟第零步完全一样
         */
        void CheckAndDoSplit()
        {
            if (_leafs.Count > _maxLeafsNumber && _field.width > _minSideLength && _field.height > _minSideLength)
                Split();
        }

        void Split()
        {
            Debug.Log("位置在((" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + "))的树梢节点达到分割条件，进行分割");

            float xCenter = (_field.left + _field.right) / 2;
            float yCenter = (_field.bottom + _field.top) / 2;

            _upperRightChild = new QuadtreeRadius<T>(_field.top, _field.right, yCenter, xCenter, _maxLeafsNumber, _minSideLength, this);
            _lowerRightChild = new QuadtreeRadius<T>(yCenter, _field.right, _field.bottom, xCenter, _maxLeafsNumber, _minSideLength, this);
            _lowerLeftChild = new QuadtreeRadius<T>(yCenter, xCenter, _field.bottom, _field.left, _maxLeafsNumber, _minSideLength, this);
            _upperLeftChild = new QuadtreeRadius<T>(_field.top, xCenter, yCenter, _field.left, _maxLeafsNumber, _minSideLength, this);

            foreach (QuadtreeLeafRadius<T> leaf in _leafs)      //因为这里向子节点存入所有叶子，最大半径的叶子依然在范围里，不需要对最大半径做特别处理
                SetLeafToChildren(leaf);
            _leafs = null;
        }

        /*
         *  碰撞检测，比第零步多了个叶子半径，原理还是根据距离做判断
         */
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

            foreach (QuadtreeLeafRadius<T> leaf in _leafs)
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

        T[] GetCollisionObjectsFromAChild(Vector2 checkPoint, float checkRadius, QuadtreeRadius<T> child)
        {
            if (child._field.PointToFieldDistance(checkPoint) <= _maxRadius + checkRadius)      //这里不光要考虑到检测半径，还要考虑到节点最大半径
                return child.CheckCollision(checkPoint, checkRadius);
            return new T[0];
        }

        /*
         *  移除叶子，比第零步多了更新最大半径的步骤和移除是否成功的返回
         */
        public bool RemoveLeaf(QuadtreeLeafRadius<T> leaf)
        {
            if (DontHaveChildren())
                return RemoveLeafFromSelf(leaf);
            else
                return RemoveLeafFromChildren(leaf);
        }

        bool RemoveLeafFromSelf(QuadtreeLeafRadius<T> leaf)
        {
            bool removeLeafBool = _leafs.Remove(leaf);
            UpdateMaxRadiusWhenRemoveLeaf();
            Debug.Log("位置在(" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + ")的树梢节点移除位置在" + leaf.position + "半径是" + leaf.radius + "的叶子，移除后的最大半径是" + _maxRadius);
            return removeLeafBool;
        }

        void UpdateMaxRadiusWhenRemoveLeaf()
        {
            float newMaxRadius = GetLeafsMaxRadiusOnRemoveLeaf();
            if (_maxRadius != newMaxRadius)             //只有在最大半径变化的时候才需要更新半径。
            {
                _maxRadius = newMaxRadius;
                CallParentUpdateMaxRadius();
            }
        }

        float GetLeafsMaxRadiusOnRemoveLeaf()
        {
            float newMaxRadius = Mathf.NegativeInfinity;    //默认值设置为负无穷，原理在开头就说过，是为了在没有叶子的时候可以直接跳过节省计算量。

            foreach (QuadtreeLeafRadius<T> leaf in _leafs)
                if (leaf.radius > newMaxRadius)
                    if (leaf.radius == _maxRadius)      //首先节点最大半径就是所有叶子里最大的半径，所以如果遍历到了和节点最大半径相同的叶子，就不会有更大的叶子了，直接return，节省计算量。
                        return _maxRadius;
                    else
                        newMaxRadius = leaf.radius;

            return newMaxRadius;
        }

        bool RemoveLeafFromChildren(QuadtreeLeafRadius<T> leaf)
        {
            Debug.Log("位置在((" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + "))的树枝节点从子节点移除位置在" + leaf.position + "半径是" + leaf.radius + "的叶子");

            if (_upperRightChild._field.Contains(leaf.position))
                return _upperRightChild.RemoveLeaf(leaf);
            if (_lowerRightChild._field.Contains(leaf.position))
                return _lowerRightChild.RemoveLeaf(leaf);
            if (_lowerLeftChild._field.Contains(leaf.position))
                return _lowerLeftChild.RemoveLeaf(leaf);
            if (_upperLeftChild._field.Contains(leaf.position))
                return _upperLeftChild.RemoveLeaf(leaf);

            Debug.LogError("位置在(" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + ")的节点，移除叶子失败，叶子不在任何一个子节点的区域里");
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

    public class QuadtreeWithRadiusField
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

        public QuadtreeWithRadiusField(float top, float right, float bottom, float left)
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
}
