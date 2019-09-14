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
