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
                        _instance = new GameObject("Quadtree").AddComponent<Quadtree>();
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

        private void Update()
        {
            _root.Update(_isSetColliders);

            //TODO：检测碰撞
        }
    }
}
