using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class MyGizmos
{
    public static void DrawCircle(Vector3 center, float radius, int edgeNumber = 360)
    {
        Vector3 beginPoint = center + Vector3.right * radius;       //三角函数角度是从正右方开始的，画圆起始点是最右边的点
        for (int i = 1; i <= edgeNumber; i++)
        {
            float angle = 2 * Mathf.PI / edgeNumber * i;

            float x = radius * Mathf.Cos(angle) + center.x;
            float y = radius * Mathf.Sin(angle) + center.y;
            Vector3 endPoint = new Vector3(x, y, center.z);

            Gizmos.DrawLine(beginPoint, endPoint);

            beginPoint = endPoint;
        }
    }
}
