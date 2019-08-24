using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider.UpdateCent
{
    /// <summary>
    /// 四叉树碰撞检测的碰撞器
    /// </summary>
    public abstract class QuadtreeCollider : MonoBehaviour
    {
        /// <summary>
        /// 最大半径，在这个半径外的碰撞器不会发生碰撞
        /// </summary>
        public abstract float maxRadius { get; }
        /// <summary>
        /// 碰撞器在世界空间内的位置
        /// </summary>
        public Vector3 position
        {
            get { return _transform.position; }
        }

        private Transform _transform;

        private void Awake()
        {
            _transform = transform;
        }
    }
}
