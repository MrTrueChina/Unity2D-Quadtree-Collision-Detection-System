using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.GizmosTools
{
    public static class GizmosTool
    {
        /// <summary>
        /// 绘制圆圈
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="radius"></param>
        /// <param name="step"></param>
        public static void DrawCircle(Vector3 center, Quaternion rotation, float radius, int step = 36)
        {
            //TODO：缺转向功能
            Gizmos.DrawSphere(center, radius);
            //var old = Gizmos.matrix;
            //if (rotation.Equals(default(Quaternion)))
            //    rotation = Quaternion.identity;
            //Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
            //var half = height / 2;

            ////draw the 4 outer lines
            //Gizmos.DrawLine(Vector3.right * radius - Vector3.up * half, Vector3.right * radius + Vector3.up * half);
            //Gizmos.DrawLine(-Vector3.right * radius - Vector3.up * half, -Vector3.right * radius + Vector3.up * half);
            //Gizmos.DrawLine(Vector3.forward * radius - Vector3.up * half, Vector3.forward * radius + Vector3.up * half);
            //Gizmos.DrawLine(-Vector3.forward * radius - Vector3.up * half, -Vector3.forward * radius + Vector3.up * half);

            ////draw the 2 cricles with the center of rotation being the center of the cylinder, not the center of the circle itself
            //DrawWireArc(center + Vector3.up * half, radius, 360, 20, rotation, center);
            //DrawWireArc(center + Vector3.down * half, radius, 360, 20, rotation, center);
            //Gizmos.matrix = old;
        }

        public static void DrawRect(Rect rect, Vector3 position, Quaternion rotation)
        {
            //TODO：缺转向功能
            Gizmos.DrawLine(new Vector3(rect.x, rect.yMax, 0), new Vector3(rect.xMax, rect.yMax, 0));
            Gizmos.DrawLine(new Vector3(rect.xMax, rect.yMax, 0), new Vector3(rect.xMax, rect.y, 0));
            Gizmos.DrawLine(new Vector3(rect.xMax, rect.y, 0), new Vector3(rect.x, rect.y, 0));
            Gizmos.DrawLine(new Vector3(rect.x, rect.y, 0), new Vector3(rect.x, rect.yMax, 0));
        }
    }
}
