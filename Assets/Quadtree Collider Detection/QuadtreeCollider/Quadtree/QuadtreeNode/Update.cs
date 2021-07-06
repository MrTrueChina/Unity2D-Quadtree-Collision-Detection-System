using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider
{
    internal partial class QuadtreeNode
    {
        /// <summary>
        /// 更新四叉树中的所有碰撞器
        /// </summary>
        internal void Update()
        {
            // 更新碰撞器位置
            UpdatePosition();

            // 更新碰撞器最大半径
            UpdateMaxRadius();
        }

        /// <summary>
        /// 更新碰撞器位置
        /// </summary>
        private void UpdatePosition()
        {
            if (HaveChildren())
            {
                // 有子节点，通知子节点更新碰撞器位置
                UpdateChildrenPosition();
            }
            else
            {
                // 没有子节点，更新当前节点内的碰撞器位置
                UpdateSelfPosition();
            }
        }

        /// <summary>
        /// 更新子节点的碰撞器位置
        /// </summary>
        private void UpdateChildrenPosition()
        {
            // 通知所有子节点更新碰撞器位置
            foreach (QuadtreeNode child in _children)
            {
                child.UpdatePosition();
            }
        }

        /// <summary>
        /// 更新当前节点内的碰撞器的位置
        /// </summary>
        private void UpdateSelfPosition()
        {
            // 移除所有当前节点保存的、已经离开当前节点范围的碰撞器，并将这些碰撞器保存起来
            List<QuadtreeCollider> outOfAreaColliders = GetAndRemoveCollidersOutOfField();

            // 把移除的碰撞器重新存入四叉树
            ResetCollidersIntoQuadtree(outOfAreaColliders);
        }

        /// <summary>
        /// 移除所有当前节点保存的、已经离开当前节点范围的碰撞器，并返回
        /// </summary>
        /// <returns></returns>
        private List<QuadtreeCollider> GetAndRemoveCollidersOutOfField()
        {
            List<QuadtreeCollider> outOfFieldColliders = new List<QuadtreeCollider>();

            // 遍历所有碰撞器，超出节点范围的保存到列表里
            foreach (QuadtreeCollider collider in _colliders)
            {
                if (!_area.Contains(collider.Position))
                {
                    outOfFieldColliders.Add(collider);
                }
            }

            // 将所有超出节点范围的碰撞器移除出四叉树
            foreach (QuadtreeCollider collider in outOfFieldColliders)
            {
                RemoveSelfColliderOnReset(collider);
            }

            // 返回被移除的碰撞器列表
            return outOfFieldColliders;
        }

        // XXX：这个方法有大问题，只是直接删除了碰撞器
        /// <summary>
        /// 从当前节点移除碰撞器，仅在移除并重新存入时使用
        /// </summary>
        /// <param name="collider"></param>
        private void RemoveSelfColliderOnReset(QuadtreeCollider collider)
        {
            _colliders.Remove(collider);

            // TODO：可以通过添加字典使封装类具有直接从树梢移除碰撞器的能力，这个方法就可以提取到包装类去了
            // TODO：移除不是完全从包装类进行，出现bug优先排查此处
        }

        /// <summary>
        /// 将碰撞器重新存入四叉树
        /// </summary>
        /// <param name="outOfFieldColliders"></param>
        private void ResetCollidersIntoQuadtree(List<QuadtreeCollider> outOfFieldColliders)
        {
            // 通过包装类将碰撞器从根节点存入
            foreach (QuadtreeCollider collider in outOfFieldColliders)
            {
                Quadtree.AddColliderOnReset(collider);
            }
        }

        /// <summary>
        /// 更新最大半径
        /// </summary>
        /// <returns></returns>
        private float UpdateMaxRadius()
        {
            if (HaveChildren())
            {
                // 有子节点，通知子节点更新最大半径，并在子节点更新后设置当前节点的最大半径
                return _maxRadius = UpdateChildrenMaxRadius();
            }
            else
            {
                // 没有子节点，更新当前节点的最大半径
                return _maxRadius = UpdateSelfMaxRadius();
            }
        }

        /// <summary>
        /// 更新子节点的最大半径
        /// </summary>
        /// <returns></returns>
        private float UpdateChildrenMaxRadius()
        {
            _maxRadius = DEFAULT_MAX_RADIUS;

            // 遍历所有子节点更新最大半径，并保留最大的半径作为当前节点的最大半径
            foreach (QuadtreeNode child in _children)
            {
                _maxRadius = Mathf.Max(_maxRadius, child.UpdateMaxRadius());
            }

            // 返回最大半径
            return _maxRadius;
        }

        /// <summary>
        /// 更新当前节点的最大半径
        /// </summary>
        /// <returns></returns>
        private float UpdateSelfMaxRadius()
        {
            _maxRadius = DEFAULT_MAX_RADIUS;

            // 遍历所有碰撞器，找出最大半径
            foreach (QuadtreeCollider collider in _colliders)
            {
                if (collider.MaxRadius > _maxRadius)
                {
                    _maxRadius = collider.MaxRadius;
                }
            }

            // 返回最大半径
            return _maxRadius;
        }
    }
}
