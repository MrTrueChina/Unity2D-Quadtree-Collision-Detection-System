using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider
{
    /// <summary>
    /// 用于接收四叉树碰撞器检测到碰撞器进入碰撞范围的接口
    /// </summary>
    public interface IOnQuadtreeCollisionEnter
    {
        /// <summary>
        /// 当四叉树碰撞器检测到碰撞器进入碰撞范围时调用本方法
        /// </summary>
        /// <param name="collider"></param>
        void OnQuadtreeCollisionEnter(QuadtreeCollider collider);
    }
}
