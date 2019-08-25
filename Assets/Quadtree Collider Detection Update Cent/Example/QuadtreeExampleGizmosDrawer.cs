using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace MtC.Tools.QuadtreeCollider.UpdateCent.Example
{
    /// <summary>
    /// 四叉树演示Gizmo绘制组件，绘制四叉树的节点范围等Gizmo
    /// </summary>
    public class QuadtreeExampleGizmosDrawer : MonoBehaviour
    {
        private void Update()
        {
            DrawGizmos();
        }

        private void DrawGizmos()
        {
            DrawQuadtreeNodesArea();
        }

        private void DrawQuadtreeNodesArea()
        {
            DrawQuadtreeNodesArea(GetQuadtreeRoot());
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

        private void DrawQuadtreeNodesArea(QuadtreeNode quadtreeNode)
        {
            if (HaveChild(quadtreeNode))
                DrawChildrenArea(quadtreeNode);
            else
                DrawArea(quadtreeNode);
        }

        private bool HaveChild(QuadtreeNode quadtreeNode)
        {
            MethodInfo method = typeof(QuadtreeNode).GetMethod("HaveChild", BindingFlags.Instance | BindingFlags.NonPublic);

            //Debug.Log("检测节点 " + quadtreeNode + " 有没有子节点：" + (bool)method.Invoke(quadtreeNode, null));

            return (bool)method.Invoke(quadtreeNode, null);
        }

        private void DrawChildrenArea(QuadtreeNode quadtreeNode)
        {
            foreach (QuadtreeNode child in GetChildren(quadtreeNode))
                DrawQuadtreeNodesArea(child);
        }

        private IEnumerable<QuadtreeNode> GetChildren(QuadtreeNode quadtreeNode)
        {
            return (List<QuadtreeNode>)typeof(QuadtreeNode).GetField("_children", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(quadtreeNode);
        }

        private void DrawArea(QuadtreeNode quadtreeNode)
        {
            //Debug.Log("绘制区域为 " + GetArea(quadtreeNode) + " 的节点的区域");

            Rect area = GetArea(quadtreeNode);

            Vector3 topRight = new Vector3(area.xMax, area.yMax, 0);
            Vector3 bottomRight = new Vector3(area.xMax, area.y, 0);
            Vector3 bottomLeft = new Vector3(area.x, area.y, 0);
            Vector3 topLeft = new Vector3(area.x, area.yMax, 0);

            Debug.DrawLine(topLeft, topRight, Color.blue * 0.8f, 0);
            Debug.DrawLine(topRight, bottomRight, Color.blue * 0.8f, 0);
            Debug.DrawLine(bottomRight, bottomLeft, Color.blue * 0.8f, 0);
            Debug.DrawLine(bottomLeft, topLeft, Color.blue * 0.8f, 0);
        }

        private Rect GetArea(QuadtreeNode quadtreeNode)
        {
            return (Rect)typeof(QuadtreeNode).GetField("_area", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(quadtreeNode);
        }
    }
}
