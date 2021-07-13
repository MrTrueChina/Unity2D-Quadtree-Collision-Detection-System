using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider
{
    internal partial class QuadtreeNode
    {
        /// <summary>
        /// 操作的返回对象
        /// </summary>
        internal class OperationResult
        {
            /// <summary>
            /// 操作成功
            /// </summary>
            public bool Success { private set; get; }
            /// <summary>
            /// 操作后影响到的碰撞器到节点的映射表
            /// </summary>
            public Dictionary<QuadtreeCollider, QuadtreeNode> CollidersToNodes { get; set; } = new Dictionary<QuadtreeCollider, QuadtreeNode>();

            public OperationResult(bool success)
            {
                Success = success;
            }
        }
    }
}
