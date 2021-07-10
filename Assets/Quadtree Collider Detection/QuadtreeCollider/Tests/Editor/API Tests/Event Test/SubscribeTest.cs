using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using System;

/// <summary>
/// 事件委托的订阅机制测试
/// </summary>
[TestFixture]
public class SubscribeTest
{
    private Action<string> actions;

    [TearDown]
    public void TearDown()
    {
        if (actions != null)
            foreach (Action<string> a in actions.GetInvocationList())
                actions -= a;
    }

    [Test]
    public void RepeatSubscribe()
    {
        actions += Say;
        actions += Say;
        actions += Say;

        Debug.Log(actions.GetInvocationList().Length == 1 ? "重复订阅不会产生多个订阅" : "重复订阅会产生多个订阅");
    }

    [Test]
    public void RepeatCancelSubscribe()
    {
        bool exception = false;

        try
        {
            actions -= Say;
        }
        catch
        {
            exception = true;
        }

        Debug.Log(exception ? "重复取消订阅会导致异常" : "重复取消订阅会导致异常");
    }

    [Test]
    public void Subscribed()
    {
        bool subscribed = false;
        actions += Say;

        foreach (Action<string> action in actions.GetInvocationList())
            if (action == Say)
                subscribed = true;

        Debug.Log(subscribed ? "使用 == 可以判断出是否已经订阅" : "使用 == 无法判断出是否已经订阅");
    }

    private void Say(string str)
    {
        Debug.Log(str);
    }
}
