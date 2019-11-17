using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// 用于测试继承了 Mono 的接口实现类的引用会不会随着 Mono 的销毁而返回null的测试类
/// </summary>
[TestFixture]
public class InterfaceOnMonoDestroyTest
{
    [UnityTest]
    public IEnumerator InterfaceOnMonoDestroy()
    {
        GameObject go = new GameObject();
        IInterfaceOnMonoDestroyTestInterface interfaceMono = go.AddComponent<InterfaceOnMonoDestroyTestMono>();
        InterfaceOnMonoDestroyTestMono mono = go.GetComponent<InterfaceOnMonoDestroyTestMono>();

        yield return null;

        Object.Destroy(go.GetComponent<InterfaceOnMonoDestroyTestMono>());

        yield return null;

        if (interfaceMono != null)
            Debug.Log("实现接口的组件被销毁后，接口引用能够通过 if( != null)");
        else
            Debug.Log("实现接口的组件被销毁后，接口引用不能通过 if( != null)");

        if (mono != null)
            Debug.Log("实现接口的组件被销毁后，组件引用能够通过 if( != null)");
        else
            Debug.Log("实现接口的组件被销毁后，组件引用不能通过 if( != null)");
    }
}
