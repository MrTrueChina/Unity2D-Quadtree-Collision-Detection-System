using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class RectExtensionTest
{
    [Test]
    public void distanceToRect_In()
    {
        Rect rect = new Rect(0, 0, 100, 100);

        Vector2[] points = new Vector2[10]
            {
                new Vector2(0,0),
                new Vector2(100,100),
                new Vector2(50,50),
                new Vector2(10,25),
                new Vector2(75,32),
                new Vector2(1.1111f,2.6554f),
                new Vector2(0.001f,99.999f),
                new Vector2(100,0),
                new Vector2(0,100),
                new Vector2(75,52)
            };
        foreach (Vector2 point in points)
            Assert.AreEqual(0, rect.pointToRectDistance(point));
    }


    [Test]
    public void distanceToRect_Out_Horizontal()
    {
        Rect rect = new Rect(0, 0, 100, 100);

        Vector2[] points = new Vector2[10]
            {
                new Vector2(0,-0.001f),
                new Vector2(100,-100),
                new Vector2(50,-50),
                new Vector2(10,-25),
                new Vector2(75,-32),
                new Vector2(1.1111f,102.6554f),
                new Vector2(0.001f,9999.999f),
                new Vector2(100,720),
                new Vector2(0,100.00001f),
                new Vector2(75,502)
            };
        float[] expectedDistances = new float[10]
            {
                0.001f,
                100,
                50,
                25,
                32,
                2.6554f,
                9899.999f,
                620,
                0.00001f,
                402
            };
        for (int i = 0; i < 10; i++)
            Assert.AreEqual(expectedDistances[i], rect.pointToRectDistance(points[i]), 0.0001f);     //Assert.AreEqual，第三个参数：允许的偏差范围。float本身存在精度问题，用这个参数来做弥补
    }


    [Test]
    public void distanceToRect_Out_Vertical()
    {
        Rect rect = new Rect(0, 0, 100, 100);

        Vector2[] points = new Vector2[10]
            {
                new Vector2(-0.0001f,0),
                new Vector2(-100,100),
                new Vector2(-50,50),
                new Vector2(-10,25),
                new Vector2(-75,32),
                new Vector2(101.1111f,2.6554f),
                new Vector2(100.001f,99.999f),
                new Vector2(200,0),
                new Vector2(1000,100),
                new Vector2(750,52)
            };
        float[] expectedDistances = new float[10]
            {
                0.0001f,
                100,
                50,
                10,
                75,
                1.1111f,
                0.001f,
                100,
                900,
                650
            };
        for (int i = 0; i < 10; i++)
            Assert.AreEqual(expectedDistances[i], rect.pointToRectDistance(points[i]), 0.0001f);
    }


    [Test]
    public void distanceToRect_Out_Oblique()
    {
        Rect rect = new Rect(0, 0, 100, 100);

        Vector2[] points = new Vector2[]
            {
                new Vector2(-1,-1),
                new Vector2(-1,101),
                new Vector2(101,-1),
                new Vector2(101,101),
                new Vector2(105,201),
            };
        float[] expectedDistances = new float[]
            {
                Mathf.Sqrt(2),
                Mathf.Sqrt(2),
                Mathf.Sqrt(2),
                Mathf.Sqrt(2),
                Mathf.Sqrt(5*5 + 101*101),
            };
        for (int i = 0; i < 5; i++)
            Assert.AreEqual(expectedDistances[i], rect.pointToRectDistance(points[i]), 0.0001f);
    }


    [Test]
    public void distanceToRectWithExtented_In()
    {
        Rect rect = new Rect(0, 0, 100, 100);

        Vector2[] points = new Vector2[]
            {
                new Vector2(-10,0),
                new Vector2(110,100),
                new Vector2(50,50),
                new Vector2(105,105),
            };
        foreach (Vector2 point in points)
            Assert.AreEqual(0, rect.pointToRectDistance(point, 10));
    }


    [Test]
    public void distanceToRectWithExtended_Out_Oblique()
    {
        Rect rect = new Rect(0, 0, 100, 100);

        Vector2[] points = new Vector2[]
            {
                new Vector2(-10,-10),       //这个方法测试一个特殊情况：点在斜方向外面，xy坐标都在延伸距离内，但点距离最近的顶点的距离超出延伸距离，此时应该求出点到顶点距离和延伸距离的差值
                new Vector2(110,110)
            };
        foreach(Vector2 point in points)
            Assert.Greater(rect.pointToRectDistance(point, 10), 0);
    }
}
