using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;

/// <summary>
/// 用于测试C#的重载功能的测试类
/// </summary>
public class OverloadTest
{
    /// <summary>
    /// 测试当存在父子级
    /// </summary>
    [Test]
    public void ParentTypeChildObject()
    {
        ParentClass child = new ChildClass();
        ParentClass brother = new BrotherClass();

        Log(child);
        Log(brother);
    }

    private void Log(ParentClass parent)
    {
        Debug.Log("父类Log，对象 = " + parent);
    }

    private void Log(ChildClass child)
    {
        Debug.Log("子类Log，对象 = " + child);
    }

    private void Log(BrotherClass brother)
    {
        Debug.Log("子类兄弟Log，对象 = " + brother);
    }

    class ParentClass { }

    class ChildClass : ParentClass { }

    class BrotherClass : ParentClass { }
}
