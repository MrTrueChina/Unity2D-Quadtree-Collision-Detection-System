using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider.UpdateCent
{
    /// <summary>
    /// 四叉树包装类
    /// </summary>
    public class Quadtree : MonoBehaviour
    {
        private static Rect _startArea = new Rect(-1, -1, 1922, 1082);

        private static Quadtree _instance;
        private static Quadtree instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                lock (typeof(Quadtree))
                {
                    if (_instance == null)
                    {
                        _instance = new GameObject("Quadtree").AddComponent<Quadtree>();
                        DontDestroyOnLoad(_instance);
                    }
                    return _instance;
                }
            }
        }

        /// <summary>
        /// 存入和取出碰撞器的操作缓存，true是存入，false是取出
        /// </summary>
        private Dictionary<QuadtreeCollider, bool> _isSetColliders = new Dictionary<QuadtreeCollider, bool>();
        /// <summary>
        /// 所有检测器
        /// </summary>
        private List<Collider> _detector = new List<Collider>();
        /// <summary>
        /// 四叉树根节点
        /// </summary>
        private QuadtreeNode _root = new QuadtreeNode(_startArea);

        /// <summary>
        /// 向四叉树中添加碰撞器
        /// </summary>
        /// <param name="collider"></param>
        public static void AddCollider(QuadtreeCollider collider)
        {
            //Debug.Log("存入碰撞器：" + collider);
            instance.DoAddCollider(collider);
        }

        private void DoAddCollider(QuadtreeCollider collider)
        {
            if (_isSetColliders.ContainsKey(collider))
                _isSetColliders[collider] = true;
            else
                _isSetColliders.Add(collider, true);
        }

        /// <summary>
        /// 从四叉树中移除碰撞器
        /// </summary>
        /// <param name="collider"></param>
        public static void RemoveCollider(QuadtreeCollider collider)
        {
            //Debug.Log("移除碰撞器：" + collider);
            instance.DoRemoveCollider(collider);
        }

        private void DoRemoveCollider(QuadtreeCollider collider)
        {
            if (_instance == null)
                return;

            if (_isSetColliders.ContainsKey(collider))
                _isSetColliders[collider] = false;
            else
                _isSetColliders.Add(collider, false);
        }

        private void Update()
        {
            UpdateQuadtree();

            //TODO：检测碰撞
        }

        private void UpdateQuadtree()
        {
            //Debug.Log("更新四叉树");

            _root.Update(_isSetColliders);

            _isSetColliders.Clear();

            //Debug.Log("清理后的存入缓存，总元素数为 " + _isSetColliders.Count + " 个");
        }
    }
}
