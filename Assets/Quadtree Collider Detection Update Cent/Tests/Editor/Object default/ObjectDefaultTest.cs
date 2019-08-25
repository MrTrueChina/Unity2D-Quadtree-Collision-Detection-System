using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;

[TestFixture]
public class ObjectDefaultTest
{
    /// <summary>
    /// 测试不能为null的对象的 default 值是同一个对象还是不同的对象
    /// </summary>
    [Test]
    public void TestObjectDefault()
    {
        Rect rect1 = default;
        Rect rect2 = default;

        Debug.Log(rect1 == rect2 ? "不能为 null 的对象的 default 是同一个对象" : "不能为 null 的对象的 default 不是同一个对象");
        Debug.Log("Rect的 default 对象是：" + rect1);
    }
}
