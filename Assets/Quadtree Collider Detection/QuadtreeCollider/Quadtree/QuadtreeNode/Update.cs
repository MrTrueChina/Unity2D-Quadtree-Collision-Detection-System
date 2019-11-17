using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider
{
    internal partial class QuadtreeNode
    {
        /// <summary>
        /// 更新四叉树中的所有节点
        /// </summary>
        internal void Update()
        {
            UpdatePosition();
            UpdateMaxRadius();
        }

        private void UpdatePosition()
        {
            if (HaveChildren())
                UpdateChildrenPosition();
            else
                UpdateSelfPosition();
        }

        private void UpdateChildrenPosition()
        {
            foreach (QuadtreeNode child in _children)
                child.UpdatePosition();
        }

        private void UpdateSelfPosition()
        {
            List<QuadtreeCollider> outOfAreaColliders = GetAndRemoveCollidersOutOfField();
            ResetCollidersIntoQuadtree(outOfAreaColliders);
        }

        private List<QuadtreeCollider> GetAndRemoveCollidersOutOfField()
        {
            List<QuadtreeCollider> outOfFieldColliders = new List<QuadtreeCollider>();

            foreach (QuadtreeCollider collider in _colliders)
                if (!_area.Contains(collider.position))
                    outOfFieldColliders.Add(collider);

            foreach (QuadtreeCollider collider in outOfFieldColliders)
                RemoveSelfColliderOnReset(collider);

            return outOfFieldColliders;
        }

        private void RemoveSelfColliderOnReset(QuadtreeCollider collider)
        {
            _colliders.Remove(collider);

            // TODO：可以通过添加字典使封装类具有直接从树梢移除碰撞器的能力，这个方法就可以提取到包装类去了
            // TODO：移除不是完全从包装类进行，出现bug优先排查此处
        }

        private void ResetCollidersIntoQuadtree(List<QuadtreeCollider> outOfFieldColliders)
        {
            foreach (QuadtreeCollider collider in outOfFieldColliders)
                Quadtree.AddColliderOnReset(collider); // 直接通过包装类从根节点存入
        }

        private float UpdateMaxRadius()
        {
            if (HaveChildren())
                return _maxRadius = UpdateChildrenMaxRadius();
            else
                return _maxRadius = UpdateSelfMaxRadius();
        }

        private float UpdateChildrenMaxRadius()
        {
            _maxRadius = DEFAULT_MAX_RADIUS;

            foreach (QuadtreeNode child in _children)
                _maxRadius = Mathf.Max(_maxRadius, child.UpdateMaxRadius());

            return _maxRadius;
        }

        private float UpdateSelfMaxRadius()
        {
            _maxRadius = DEFAULT_MAX_RADIUS;

            foreach (QuadtreeCollider collider in _colliders)
                if (collider.maxRadius > _maxRadius)
                    _maxRadius = collider.maxRadius;

            return _maxRadius;
        }
    }
}
