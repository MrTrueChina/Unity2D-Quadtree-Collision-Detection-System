﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider
{
    /// <summary>
    /// 四叉树碰撞检测器
    /// </summary>
    internal static class QuadtreeCollisionDetector
    {
        /// <summary>
        /// 弥补 switch 不能使用 type 准备的表驱动字典
        /// </summary>
        private static readonly Dictionary<Type, Dictionary<Type, Func<QuadtreeCollider, QuadtreeCollider, bool>>> colliderDictionary = new Dictionary<Type, Dictionary<Type, Func<QuadtreeCollider, QuadtreeCollider, bool>>>
        {
            {
                //第一个参数是圆形碰撞器的表驱动字典
                typeof(CircleQuadtreeCollider), new Dictionary<Type, Func<QuadtreeCollider, QuadtreeCollider, bool>>
                {
                    { typeof(CircleQuadtreeCollider), CircleToCircle }
                }
            }
        };

        /// <summary>
        /// 判断两个碰撞器是否发生碰撞
        /// </summary>
        /// <param name="colliderA"></param>
        /// <param name="colliderB"></param>
        /// <returns></returns>
        internal static bool IsCollition(QuadtreeCollider colliderA, QuadtreeCollider colliderB)
        {
            return colliderDictionary[colliderA.GetType()][colliderB.GetType()](colliderA, colliderB);
        }

        /// <summary>
        /// 判断圆形碰撞器和圆形碰撞器是否发生碰撞
        /// </summary>
        /// <param name="colliderA"></param>
        /// <param name="colliderB"></param>
        /// <returns></returns>
        private static bool CircleToCircle(QuadtreeCollider colliderA, QuadtreeCollider colliderB)
        {
            CircleQuadtreeCollider circleColliderA = (CircleQuadtreeCollider)colliderA;
            CircleQuadtreeCollider circleColliderB = (CircleQuadtreeCollider)colliderB;

            return Vector2.Distance(circleColliderA.Position, circleColliderB.Position) <= circleColliderA.Radius + circleColliderB.Radius;
        }
    }
}
