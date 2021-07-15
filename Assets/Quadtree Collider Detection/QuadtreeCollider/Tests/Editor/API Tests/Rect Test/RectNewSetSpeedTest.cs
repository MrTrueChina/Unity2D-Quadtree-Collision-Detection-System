using NUnit.Framework;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider.Test
{
    /// <summary>
    /// 测试Rect的new和set两个方法的速度
    /// </summary>
    public class RectNewSetSpeedTest
    {
        [Test]
        public void NewSetSpeed()
        {
            const int LOOP_TIME = 10000000;

            long newTime;
            long setTime;

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            Rect rect = new Rect();

            stopwatch.Start();
            for (int i = 0; i < LOOP_TIME; i++)
                rect = new Rect(i, i, i, i);
            stopwatch.Stop();
            newTime = stopwatch.ElapsedMilliseconds;
            Debug.Log("循环创建Rect " + LOOP_TIME + " 次，耗时 " + newTime + " 毫秒");

            stopwatch.Reset();

            stopwatch.Start();
            for (int i = 0; i < LOOP_TIME; i++)
                rect.Set(i, i, i, i);
            stopwatch.Stop();
            setTime = stopwatch.ElapsedMilliseconds;
            Debug.Log("向Rect存值 " + LOOP_TIME + " 次，耗时 " + setTime + " 毫秒");

            Debug.Log(newTime > setTime ? "向已有Rect存值比创建新Rect快" : "创建新Rect比向已有Rect存值要快");
        }
    }
}
