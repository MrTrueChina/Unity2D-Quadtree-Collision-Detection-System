using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider
{
    /// <summary>
    /// 绘制四叉树的组件
    /// </summary>
    public class QuadtreeDrawer : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            // 如果在游戏状态，绘制四叉树
            if (Application.isPlaying)
            {
                DrawQuadtreeNodes(GetQuadtreeRoot());
            }
        }

        /// <summary>
        /// 获取四叉树根节点
        /// </summary>
        /// <returns></returns>
        private QuadtreeNode GetQuadtreeRoot()
        {
            //Debug.Log("获取根节点，获取结果：" + (QuadtreeNode)typeof(Quadtree).GetField("root", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(GetQuadtreeInstance()));

            return (QuadtreeNode)typeof(Quadtree).GetField("root", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(GetQuadtreeInstance());
        }

        /// <summary>
        /// 获取四叉树实例对象
        /// </summary>
        /// <returns></returns>
        private Quadtree GetQuadtreeInstance()
        {
            return (Quadtree)typeof(Quadtree).GetField("instance", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        }

        /// <summary>
        /// 绘制四叉树指定节点
        /// </summary>
        /// <param name="quadtreeNode"></param>
        private void DrawQuadtreeNodes(QuadtreeNode quadtreeNode)
        {
            if (HaveChildren(quadtreeNode))
            {
                // 有子节点的情况下，绘制子节点
                DrawChildrenNode(quadtreeNode);
            }
            else
            {
                // 没有子节点，绘制末端节点
                DrawTipNode(quadtreeNode);
            }

            // 绘制节点
            DrawNode(quadtreeNode);
        }

        /// <summary>
        /// 判断一个四叉树节点是否有子节点
        /// </summary>
        /// <param name="quadtreeNode"></param>
        /// <returns></returns>
        private bool HaveChildren(QuadtreeNode quadtreeNode)
        {
            MethodInfo method = typeof(QuadtreeNode).GetMethod("HaveChildren", BindingFlags.Instance | BindingFlags.NonPublic);

            //Debug.Log("检测节点 " + quadtreeNode + " 有没有子节点：" + (bool)method.Invoke(quadtreeNode, null));

            return (bool)method.Invoke(quadtreeNode, null);
        }

        /// <summary>
        /// 绘制一个四叉树节点的所有子节点
        /// </summary>
        /// <param name="quadtreeNode"></param>
        private void DrawChildrenNode(QuadtreeNode quadtreeNode)
        {
            // 遍历所有子节点并绘制
            foreach (QuadtreeNode child in GetChildren(quadtreeNode))
            {
                DrawQuadtreeNodes(child);
            }
        }

        /// <summary>
        /// 获取一个四叉树节点的所有子节点
        /// </summary>
        /// <param name="quadtreeNode"></param>
        /// <returns></returns>
        private IEnumerable<QuadtreeNode> GetChildren(QuadtreeNode quadtreeNode)
        {
            return (List<QuadtreeNode>)typeof(QuadtreeNode).GetField("children", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(quadtreeNode);
        }

        /// <summary>
        /// 绘制末端节点
        /// </summary>
        /// <param name="quadtreeNode"></param>
        private void DrawTipNode(QuadtreeNode quadtreeNode)
        {
            // 根据节点设置颜色
            Gizmos.color = GetNodeColor(quadtreeNode);
            Handles.color = GetNodeColor(quadtreeNode);

            // 绘制节点区域
            DrawArea(quadtreeNode);

            // 绘制节点中的碰撞器
            DrawColliders(quadtreeNode);
        }

        /// <summary>
        /// 根据节点位置获取节点颜色
        /// </summary>
        /// <param name="quadtreeNode"></param>
        /// <returns></returns>
        private Color GetNodeColor(QuadtreeNode quadtreeNode)
        {
            // 获取根节点范围和要获取颜色的节点的范围，后续根据这两个范围确认颜色
            Rect rootArea = GetArea(GetQuadtreeRoot());
            Rect area = GetArea(quadtreeNode);

            // 计算要获取颜色的节点的位置在根节点范围中的比例
            float xLerp = Mathf.InverseLerp(rootArea.xMin, rootArea.xMax, area.x);
            float yLerp = Mathf.InverseLerp(rootArea.yMin, rootArea.yMax, area.y);

            // 通过位置比例返回颜色，越靠右越红，越靠上越绿
            return new Color(xLerp, yLerp, 0.5f);
        }

        /// <summary>
        /// 获取指定节点的区域
        /// </summary>
        /// <param name="quadtreeNode"></param>
        /// <returns></returns>
        private Rect GetArea(QuadtreeNode quadtreeNode)
        {
            return (Rect)typeof(QuadtreeNode).GetField("area", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(quadtreeNode);
        }

        /// <summary>
        /// 绘制节点区域
        /// </summary>
        /// <param name="quadtreeNode"></param>
        private void DrawArea(QuadtreeNode quadtreeNode)
        {
            //Debug.Log("绘制区域为 " + GetArea(quadtreeNode) + " 的节点的区域");

            // 获取区域
            Rect area = GetArea(quadtreeNode);

            // 计算出四个顶点
            Vector3 topRight = new Vector3(area.xMax, area.yMax, 0);
            Vector3 bottomRight = new Vector3(area.xMax, area.y, 0);
            Vector3 bottomLeft = new Vector3(area.x, area.y, 0);
            Vector3 topLeft = new Vector3(area.x, area.yMax, 0);

            // 连线，画出区域
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);
        }

        /// <summary>
        /// 绘制一个节点中所有的碰撞器
        /// </summary>
        /// <param name="quadtreeNode"></param>
        private void DrawColliders(QuadtreeNode quadtreeNode)
        {
            // 遍历碰撞器并绘制
            foreach (QuadtreeCollider collider in GetColliders(quadtreeNode))
            {
                DrawCollider(collider);
            }
        }

        /// <summary>
        /// 绘制单个碰撞器
        /// </summary>
        /// <param name="collider"></param>
        private void DrawCollider(QuadtreeCollider collider)
        {
            Vector3 beginPoint = collider.transform.position + Vector3.right * collider.MaxRadius * Mathf.Max(Mathf.Abs(collider.transform.lossyScale.x), Mathf.Abs(collider.transform.lossyScale.y));       //三角函数角度是从正右方开始的，画圆起始点是最右边的点raw
            Gizmos.DrawLine(collider.transform.position, beginPoint);
            for (int i = 1; i <= 144; i++)
            {
                float angle = 2 * Mathf.PI / 144 * i;

                float x = collider.MaxRadius * Mathf.Max(Mathf.Abs(collider.transform.lossyScale.x), Mathf.Abs(collider.transform.lossyScale.y)) * Mathf.Cos(angle) + collider.transform.position.x;
                float y = collider.MaxRadius * Mathf.Max(Mathf.Abs(collider.transform.lossyScale.x), Mathf.Abs(collider.transform.lossyScale.y)) * Mathf.Sin(angle) + collider.transform.position.y;
                Vector3 endPoint = new Vector3(x, y, collider.transform.position.z);

                Gizmos.DrawLine(beginPoint, endPoint);

                beginPoint = endPoint;
            }
        }

        /// <summary>
        /// 获取一个节点的所有碰撞器
        /// </summary>
        /// <param name="quadtreeNode"></param>
        /// <returns></returns>
        private List<QuadtreeCollider> GetColliders(QuadtreeNode quadtreeNode)
        {
            return (List<QuadtreeCollider>)typeof(QuadtreeNode).GetField("colliders", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(quadtreeNode);
        }

        /// <summary>
        /// 绘制节点
        /// </summary>
        /// <param name="node"></param>
        private void DrawNode(QuadtreeNode node)
        {
            Gizmos.color = GetNodeColor(node);

            // 绘制节点信息
            DrawNodeInfomationText(node);
        }

        /// <summary>
        /// 绘制节点信息
        /// </summary>
        /// <param name="node"></param>
        private void DrawNodeInfomationText(QuadtreeNode node)
        {
            Handles.Label(GetArea(node).center, GetNodeInfomationText(node));
        }

        /// <summary>
        /// 获取一个节点的信息文本
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private string GetNodeInfomationText(QuadtreeNode node)
        {
            string infomation = "";

            infomation += "总碰撞器数：" + GetColliders(node).Count + "\n";
            infomation += "最大检测半径：" + GetMaxRadius(node) + "\n";

            return infomation;
        }

        /// <summary>
        /// 获取最大半径
        /// </summary>
        /// <param name="quadtreeNode"></param>
        /// <returns></returns>
        private float GetMaxRadius(QuadtreeNode quadtreeNode)
        {
            return (float)typeof(QuadtreeNode).GetField("maxRadius", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(quadtreeNode);
        }
    }
}
