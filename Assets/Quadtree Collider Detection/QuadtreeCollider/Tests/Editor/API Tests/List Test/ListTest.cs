using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using System.Linq;

namespace MtC.Tools.QuadtreeCollider.Test
{
    [TestFixture]
    public class ListTest
    {
        /// <summary>
        /// 测试List和LLinkedList的foreach速度
        /// </summary>
        [Test]
        public void ForeachSpeed()
        {
            List<int> list = new List<int>();
            LinkedList<int> linkedList = new LinkedList<int>();

            for (int i = 0; i < 1000000; i++)
            {
                list.Add(i);
                linkedList.AddLast(i);
            }

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            stopwatch.Start();

            foreach (int value in list)
                continue;

            stopwatch.Stop();

            Debug.Log("list：" + stopwatch.ElapsedMilliseconds);

            stopwatch.Start();

            foreach (int value in linkedList)
                continue;

            stopwatch.Stop();

            Debug.Log("linkedlist：" + stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// 测试 List 的 Remove 方法会不会导致总数减少
        /// </summary>
        [Test]
        public void RemoveCount()
        {
            List<int> list = new List<int> { 1, 2, 3 };

            list.Remove(1);

            Debug.Log(list.Count == 2 ? "List.Remove 会导致总数减少" : "List.Remove不会导致总数减少");
        }

        /// <summary>
        /// 测试 Distinct 方法会不会导致原列表变化
        /// </summary>
        [Test]
        public void Distinct()
        {
            List<string> list = new List<string> { "Hello", "World", "Hello" };

            List<string> distinctList = new List<string>(list.Distinct());

            Debug.Log(list.Count == 2 ? "list.Distinct 会改变原列表" : "list.Distinct 不会改变原列表");
        }
    }
}
