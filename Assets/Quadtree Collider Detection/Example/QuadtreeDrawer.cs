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
            if (Application.isPlaying)
                DrawQuadtreeNodes(GetQuadtreeRoot());
        }

        private QuadtreeNode GetQuadtreeRoot()
        {
            //Debug.Log("获取根节点，获取结果：" + (QuadtreeNode)typeof(Quadtree).GetField("_root", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(GetQuadtreeInstance()));

            return (QuadtreeNode)typeof(Quadtree).GetField("_root", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(GetQuadtreeInstance());
        }

        private Quadtree GetQuadtreeInstance()
        {
            return (Quadtree)typeof(Quadtree).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        }

        private void DrawQuadtreeNodes(QuadtreeNode quadtreeNode)
        {
            if (HaveChildren(quadtreeNode))
                DrawChildrenNode(quadtreeNode);
            else
                DrawTipNode(quadtreeNode);

            DrawNode(quadtreeNode);
        }

        private bool HaveChildren(QuadtreeNode quadtreeNode)
        {
            MethodInfo method = typeof(QuadtreeNode).GetMethod("HaveChildren", BindingFlags.Instance | BindingFlags.NonPublic);

            //Debug.Log("检测节点 " + quadtreeNode + " 有没有子节点：" + (bool)method.Invoke(quadtreeNode, null));

            return (bool)method.Invoke(quadtreeNode, null);
        }

        private void DrawChildrenNode(QuadtreeNode quadtreeNode)
        {
            foreach (QuadtreeNode child in GetChildren(quadtreeNode))
                DrawQuadtreeNodes(child);
        }

        private IEnumerable<QuadtreeNode> GetChildren(QuadtreeNode quadtreeNode)
        {
            return (List<QuadtreeNode>)typeof(QuadtreeNode).GetField("_children", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(quadtreeNode);
        }

        private void DrawTipNode(QuadtreeNode quadtreeNode)
        {
            Gizmos.color = GetNodeColor(quadtreeNode);
            Handles.color = GetNodeColor(quadtreeNode);

            DrawArea(quadtreeNode);
            DrawColliders(quadtreeNode);
        }

        private Color GetNodeColor(QuadtreeNode quadtreeNode)
        {
            Rect area = GetArea(quadtreeNode);

            float xLerp = Mathf.InverseLerp(QuadtreeConfig.startArea.xMin, QuadtreeConfig.startArea.xMax, area.x);
            float yLerp = Mathf.InverseLerp(QuadtreeConfig.startArea.yMin, QuadtreeConfig.startArea.yMax, area.y);

            return new Color(xLerp, yLerp, 0.5f);
        }

        private Rect GetArea(QuadtreeNode quadtreeNode)
        {
            return (Rect)typeof(QuadtreeNode).GetField("_area", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(quadtreeNode);
        }

        private void DrawArea(QuadtreeNode quadtreeNode)
        {
            //Debug.Log("绘制区域为 " + GetArea(quadtreeNode) + " 的节点的区域");

            Rect area = GetArea(quadtreeNode);

            Vector3 topRight = new Vector3(area.xMax, area.yMax, 0);
            Vector3 bottomRight = new Vector3(area.xMax, area.y, 0);
            Vector3 bottomLeft = new Vector3(area.x, area.y, 0);
            Vector3 topLeft = new Vector3(area.x, area.yMax, 0);

            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);
        }

        private void DrawColliders(QuadtreeNode quadtreeNode)
        {
            foreach (QuadtreeCollider collider in GetColliders(quadtreeNode))
                DrawCollider(collider);
        }

        private void DrawCollider(QuadtreeCollider collider)
        {
            Vector3 beginPoint = collider.transform.position + Vector3.right * collider.maxRadius * Mathf.Max(Mathf.Abs(collider.transform.lossyScale.x), Mathf.Abs(collider.transform.lossyScale.y));       //三角函数角度是从正右方开始的，画圆起始点是最右边的点raw
            Gizmos.DrawLine(collider.transform.position, beginPoint);
            for (int i = 1; i <= 144; i++)
            {
                float angle = 2 * Mathf.PI / 144 * i;

                float x = collider.maxRadius * Mathf.Max(Mathf.Abs(collider.transform.lossyScale.x), Mathf.Abs(collider.transform.lossyScale.y)) * Mathf.Cos(angle) + collider.transform.position.x;
                float y = collider.maxRadius * Mathf.Max(Mathf.Abs(collider.transform.lossyScale.x), Mathf.Abs(collider.transform.lossyScale.y)) * Mathf.Sin(angle) + collider.transform.position.y;
                Vector3 endPoint = new Vector3(x, y, collider.transform.position.z);

                Gizmos.DrawLine(beginPoint, endPoint);

                beginPoint = endPoint;
            }
        }

        private List<QuadtreeCollider> GetColliders(QuadtreeNode quadtreeNode)
        {
            return (List<QuadtreeCollider>)typeof(QuadtreeNode).GetField("_colliders", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(quadtreeNode);
        }

        private void DrawNode(QuadtreeNode node)
        {
            Gizmos.color = GetNodeColor(node);

            DrawNodeInfomationText(node);
        }

        private void DrawNodeInfomationText(QuadtreeNode node)
        {
            Handles.Label(GetArea(node).center, GetNodeInfomationText(node));
        }

        private string GetNodeInfomationText(QuadtreeNode node)
        {
            string infomation = "";

            infomation += "总碰撞器数：" + GetColliders(node).Count + "\n";
            infomation += "最大检测半径：" + GetMaxRadius(node) + "\n";

            return infomation;
        }

        private float GetMaxRadius(QuadtreeNode quadtreeNode)
        {
            return (float)typeof(QuadtreeNode).GetField("_maxRadius", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(quadtreeNode);
        }
    }
}
