using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;

[TestFixture]
public class ListTest
{
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
}
