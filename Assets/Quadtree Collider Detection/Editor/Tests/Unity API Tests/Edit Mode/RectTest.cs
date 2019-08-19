using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;

[TestFixture]
public class RectTest
{
    private const float WIDTH = 10f;
    private const float HEIGHT = 25f;
    private const float X = 10f;
    private const float Y = 2f;

    private Rect _rect;

    [SetUp]
    public void SetUp()
    {
        _rect = new Rect(X, Y, WIDTH, HEIGHT);
    }

    /// <summary>
    /// 测试Rect的Contains，最右边的点是否不包含在Rect中
    /// </summary>
    [Test]
    public void Contains_RightBorder_False()
    {
        Vector2 right = new Vector2(X + WIDTH, (Y + HEIGHT) / 2);

        Assert.IsFalse(_rect.Contains(right));
    }
}
