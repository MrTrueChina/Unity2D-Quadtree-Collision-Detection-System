/*
 *  增加向上生长功能，在存入时如果超出整棵树的范围则向着叶子方向反向生长树，更新也是先移除后存入，一起在存入环节处理
 *  
 *  向上生长后根节点发生变化，但四叉树所有外部操作都是从根节点开始，为了解决这个矛盾点大概有三个方法：
 *      1.每个可能造成根节点变化的操作都返回新的根节点
 *      2.每个可能造成根节点变化的操作都用出参数返回新的根节点
 *      3.增加一层，在这一层里将操作转为从根节点开始
 *      
 *  在经过短暂的思想斗争后，我个人不能接受一个方法同时进行本职工作和返回新根节点两个操作，于是我选择增加一层
 */

using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.Quadtree.Example.Step6Upwards
{
    public class QuadtreeUpwards : MonoBehaviour
    {
        static QuadtreeUpwards quadtreeObject
        {
            get
            {
                if (_quadtreeObject != null)
                    return _quadtreeObject;

                _quadtreeObject = new GameObject("Quadtree").AddComponent<QuadtreeUpwards>();
                return _quadtreeObject;
            }
        }
        static QuadtreeUpwards _quadtreeObject;

        QuadtreeDataUpwards<GameObject> _quadtree;

        //初始化
        private void Awake()
        {
            QuadtreeSettingUpwards setting = Resources.Load<QuadtreeSettingUpwards>("QuadtreeCanUpwardsSetting");
            _quadtree = new QuadtreeDataUpwards<GameObject>(setting.startTop, setting.startRight, setting.startBottom, setting.startLeft, setting.maxLeafsNumber, setting.minSideLength);
        }

        //存入
        public static void SetLeaf(QuadtreeDataUpwards<GameObject>.Leaf leaf)
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

        public static GameObject[] CheckCollision(QuadtreeDataUpwards<GameObject>.Leaf leaf)
        {
            if (_quadtreeObject != null)
                return quadtreeObject._quadtree.CheckCollision(leaf);
            return new GameObject[0];
        }

        //移除
        public static bool RemoveLeaf(QuadtreeDataUpwards<GameObject>.Leaf leaf)
        {
            if (_quadtreeObject != null)
                return _quadtreeObject._quadtree.RemoveLeaf(leaf);
            return false;
        }
    }

    public class QuadtreeDataUpwards<T>
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

        QuadtreeDataUpwards<T> _root;
        QuadtreeDataUpwards<T> _parent;
        QuadtreeDataUpwards<T> _upperRightChild;
        QuadtreeDataUpwards<T> _lowerRightChild;
        QuadtreeDataUpwards<T> _lowerLeftChild;
        QuadtreeDataUpwards<T> _upperLeftChild;

        List<Leaf> _leafs = new List<Leaf>();

        int _maxLeafsNumber;
        float _minSideLength;

        public QuadtreeDataUpwards(float top, float right, float bottom, float left, int maxLeafNumber, float minSideLength, QuadtreeDataUpwards<T> root = null, QuadtreeDataUpwards<T> parent = null)
        {
            _field = new Field(top, right, bottom, left);

            _maxLeafsNumber = maxLeafNumber;
            _minSideLength = minSideLength;

            _root = root != null ? root : this;

            _parent = parent;
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
            Debug.Log("<color=#0040A0>位置在" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + "的树梢节点存入位置在" + leaf.position + "半径是" + leaf.radius + "的叶子，存入后的最大半径是" + _maxRadius + "</color>");
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
                Debug.Log("<color=#A000A0>位置在" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + "的树枝节点更新最大半径，更新后的最大半径是" + _maxRadius + "</color>");
                CallParentUpdateMaxRadius();
            }
        }

        float GetChildrenMaxRadius()
        {
            return Mathf.Max(_upperRightChild._maxRadius, _lowerRightChild._maxRadius, _lowerLeftChild._maxRadius, _upperLeftChild._maxRadius);
        }

        bool SetLeafToChildren(Leaf leaf)
        {
            Debug.Log("<color=#0040A0>位置在" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + "的树枝节点向子节点存入位置在" + leaf.position + "半径是" + leaf.radius + "的叶子</color>");
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
            Debug.Log("<color=#808000>位置在" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + "的树梢节点达到分割条件，进行分割</color>");

            DoUpdate();

            float xCenter = (_field.left + _field.right) / 2;
            float yCenter = (_field.bottom + _field.top) / 2;

            _upperRightChild = new QuadtreeDataUpwards<T>(_field.top, _field.right, yCenter, xCenter, _maxLeafsNumber, _minSideLength, _root, this);
            _lowerRightChild = new QuadtreeDataUpwards<T>(yCenter, _field.right, _field.bottom, xCenter, _maxLeafsNumber, _minSideLength, _root, this);
            _lowerLeftChild = new QuadtreeDataUpwards<T>(yCenter, xCenter, _field.bottom, _field.left, _maxLeafsNumber, _minSideLength, _root, this);
            _upperLeftChild = new QuadtreeDataUpwards<T>(_field.top, xCenter, yCenter, _field.left, _maxLeafsNumber, _minSideLength, _root, this);

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

            QuadtreeDataUpwards<T> newRoot = new QuadtreeDataUpwards<T>(newTop, newRight, newBottom, newLeft, _maxLeafsNumber, _minSideLength);      //新根节点

            //右上节点，需要存入的情况是向左下方生长，即 x < 0 && y < 0
            if (growthDirection.x >= 0 || growthDirection.y >= 0)       //只要不满足向左下方生长的条件就用创建
                newRoot._upperRightChild = new QuadtreeDataUpwards<T>(newTop, newRight, newYCenter, newXCenter, _maxLeafsNumber, _minSideLength, newRoot, newRoot);
            else
                newRoot._upperRightChild = this;

            //右下节点，需要存入的情况是向左上方生长，即 x <0 && y >= 0
            if (growthDirection.x >= 0 || growthDirection.y < 0)
                newRoot._lowerRightChild = new QuadtreeDataUpwards<T>(newYCenter, newRight, newBottom, newXCenter, _maxLeafsNumber, _minSideLength, newRoot, newRoot);
            else
                newRoot._lowerRightChild = this;

            //左下节点，需要存入的情况是向右上方生长，即 x >= 0 && y >= 0
            if (growthDirection.x < 0 || growthDirection.y < 0)
                newRoot._lowerLeftChild = new QuadtreeDataUpwards<T>(newYCenter, newXCenter, newBottom, newLeft, _maxLeafsNumber, _minSideLength, newRoot, newRoot);
            else
                newRoot._lowerLeftChild = this;

            //左上节点，需要存入的情况是向右下方生长，即 x >= 0 && y < 0
            if (growthDirection.x < 0 || growthDirection.y >= 0)
                newRoot._upperLeftChild = new QuadtreeDataUpwards<T>(newTop, newXCenter, newYCenter, newLeft, _maxLeafsNumber, _minSideLength, newRoot, newRoot);
            else
                newRoot._upperLeftChild = this;

            _parent = newRoot;              //因为每次向上生长都是由现在的根节点调用的，新的根节点生长完成后旧的根节点的父节点就是新的根节点
            newRoot.UpdateRoot(newRoot);

            Debug.Log("<color=#008510>位置在" + leafPosition + "的叶子存入树，树向" + (growthDirection.x >= 0 ? (growthDirection.y >= 0 ? "右上方" : "右下方") : (growthDirection.y >= 0 ? "左上方" : "左下方")) + "生长，生长后的树的范围是 " + newRoot._field.top + "  " + newRoot._field.right + " " + newRoot._field.bottom + "  " + newRoot._field.left + "</color>");
        }

        void UpdateRoot(QuadtreeDataUpwards<T> root)
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

            DrawField();    //绘制节点范围，删除不影响功能
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
            Debug.Log("<color=#800080>位置在" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + "的树梢节点移除位置在" + leaf.position + "半径是" + leaf.radius + "的叶子，重新存入树</color>");
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

        T[] GetCollisionObjectsFromAChild(Vector2 checkPoint, float checkRadius, QuadtreeDataUpwards<T> child)
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
                //Debug.Log("<color=#802030>位置在" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + "的树梢节点移除位置在" + leaf.position + "半径是" + leaf.radius + "的叶子，移除后的最大半径是" + _maxRadius + "</color>");
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
                //Debug.Log("<color=#108010>位置在" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + "的树梢节点半径发生变化，新半径是" + _maxRadius + "</color>");
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
            //Debug.Log("<color=#802030>位置在" + _field.top + "," + _field.right + "," + _field.bottom + "," + _field.left + "的树枝节点从子节点移除位置在" + leaf.position + "半径是" + leaf.radius + "的叶子</color>");
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
}
