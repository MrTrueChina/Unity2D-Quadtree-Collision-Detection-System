using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider
{
    // 从树中移除碰撞器的部分
    internal partial class QuadtreeNode
    {
        /// <summary>
        /// 从树中移除碰撞器
        /// </summary>
        /// <param name="collider"></param>
        internal void RemoveCollider(QuadtreeCollider collider)
        {
            if (!RemoveColliderByPosition(collider)) // 首先根据位置移除碰撞器，但有时候碰撞器移出了所在节点的范围，就会发生找不到节点无法移除的情况
                RemoveColliderFromAllNodes(collider); // 此时使用全节点遍历移除
        }

        private bool RemoveColliderByPosition(QuadtreeCollider collider)
        {
            if (HaveChildren())
                return RemoveColliderFromChildrenByPosition(collider);

            return RemoveColliderFromSelfByPosition(collider);
        }

        private bool RemoveColliderFromChildrenByPosition(QuadtreeCollider collider)
        {
            foreach (QuadtreeNode child in _children)
                if (child.RemoveColliderByPosition(collider))
                    return true;

            return false;
        }

        private bool RemoveColliderFromSelfByPosition(QuadtreeCollider collider)
        {
            if (_area.Contains(collider.position))
                return RemoveColliderFromSelf(collider);

            return false;
        }

        private bool RemoveColliderFromSelf(QuadtreeCollider collider)
        {
            return _colliders.Remove(collider);
        }

        private bool RemoveColliderFromAllNodes(QuadtreeCollider collider)
        {
            if (HaveChildren())
                return RemoveColliderFromChildrenAndAllNodes(collider);

            return RemoveColliderFromSelf(collider);
        }

        private bool RemoveColliderFromChildrenAndAllNodes(QuadtreeCollider collider)
        {
            foreach (QuadtreeNode child in _children)
                if (child.RemoveColliderFromAllNodes(collider))
                    return true;

            return false;
        }
    }
}
