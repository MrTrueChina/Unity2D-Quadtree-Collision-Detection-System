using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider
{
    /// <summary>
    /// 四叉树单例部分
    /// </summary>
    internal partial class Quadtree : MonoBehaviour
    {
        /// <summary>
        /// 实例
        /// </summary>
        private static Quadtree Instance
        {
            get
            {
                if (instance != null)
                    return instance;

                lock (typeof(Quadtree))
                {
                    if (instance == null)
                    {
                        // 创建一个带四叉树组件的对象，并设为不随场景加载销毁
                        instance = new GameObject("Quadtree").AddComponent<Quadtree>();
                        DontDestroyOnLoad(instance);
                    }
                    return instance;
                }
            }
        }
        private static Quadtree instance;

        /// <summary>
        /// 向四叉树中添加碰撞器
        /// </summary>
        /// <param name="collider"></param>
        public static QuadtreeNode.OperationResult AddCollider(QuadtreeCollider collider)
        {
            // 不能重复存入碰撞器
            if (Instance.collidersToNodes.ContainsKey(collider))
            {
                return new QuadtreeNode.OperationResult(false);
            }

            // 向实例中添加碰撞器
            return Instance.DoAddCollider(collider);
        }

        /// <summary>
        /// 从四叉树中移除碰撞器，符合条件时会合并节点
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        public static QuadtreeNode.OperationResult RemoveColliderWithMerge(QuadtreeCollider collider)
        {
            return RemoveCollider(collider, true);
        }

        /// <summary>
        /// 从四叉树中移除碰撞器，不进行合并
        /// </summary>
        /// <param name="collider"></param>
        internal static QuadtreeNode.OperationResult RemoveColliderWithOutMerge(QuadtreeCollider collider)
        {
            return RemoveCollider(collider, false);
        }

        /// <summary>
        /// 从四叉树中移除碰撞器
        /// </summary>
        /// <param name="collider"></param>
        /// <param name="withMerge">是否需要在需要合并的时候进行合并</param>
        /// <returns></returns>
        internal static QuadtreeNode.OperationResult RemoveCollider(QuadtreeCollider collider, bool withMerge)
        {
            // 如果没有实例，不进行处理，这一步是必须的，否则在游戏关闭时会发生销毁时四叉树实例一次次出现，进而导致异常
            if(instance == null)
            {
                return new QuadtreeNode.OperationResult(false);
            }

            // 映射表里没有这个碰撞器，说明树里没有这个碰撞器，直接返回失败
            if (!Instance.collidersToNodes.ContainsKey(collider))
            {
                return new QuadtreeNode.OperationResult(false);
            }

            // 根据映射表直接从末梢节点移除碰撞器
            QuadtreeNode.OperationResult result;
            if (withMerge)
            {
                result = Instance.collidersToNodes[collider].RemoveColliderFromSelfWithMerge(collider);
            }
            else
            {
                result = Instance.collidersToNodes[collider].RemoveColliderFromSelfWithOutMerge(collider);
            }

            if (result.Success)
            {
                // 移除成功后更新映射表，覆盖合并映射表并移除空值
                Instance.collidersToNodes.OverlayMerge(result.CollidersToNodes).RemoveOnValueIsNull();
            }
            else
            {
                throw new System.ArgumentOutOfRangeException(
                    "移除碰撞器 "
                    + "(" + collider.Position.x + ", " + collider.Position.y + ")"
                    + " 时发生错误：碰撞器到节点的映射表中存在这个碰撞器，但映射到的节点  "
                    + "(" + Instance.collidersToNodes[collider].Area.ToString() + ")"
                    + " 移除失败，可能是碰撞器并不在节点中");
            }

            return result;
        }
    }
}
