using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider
{
    // 从树中移除碰撞器的部分
    internal partial class QuadtreeNode
    {
        /// <summary>
        /// 从当前节点中移除指定碰撞器
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        public OperationResult RemoveColliderFromSelf(QuadtreeCollider collider)
        {
            bool listResult = colliders.Remove(collider);

            if (listResult)
            {
                // 创建操作成功的返回对象
                OperationResult result = new OperationResult(true);

                // 映射表值为 null 表示碰撞器不属于任何节点
                result.CollidersToNodes.Add(collider, null);

                return result;
            }
            else
            {
                // 返回移除失败，移除失败不改变映射表，直接返回失败即可
                return new OperationResult(false);
            }
        }
    }
}
