//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using NUnit.Framework;
//using System.Reflection;
//using System;
//using MtC.Tools.QuadtreeCollider;

// 此测试依赖于旧版本四叉树，测试结果正确，测试的代码不会更新

//[TestFixture]
//public class QuadTreeTest
//{
//    /// <summary>
//    /// 测试四叉树创建子节点方法创建的子节点的中央分隔线是否紧贴，如果上下左右子节点的范围的边缘没有重合，则代表四叉树有空隙，落入空隙的碰撞器将无法存入树
//    /// </summary>
//    [Test]
//    [TestCase(1f, 2f, 4f, 3f)]
//    [TestCase(2.0649f, 4.97412f, 264.94125f, 23.64985f)]
//    [TestCase(0.0001f, 0.0001f, 0.001f, 0.0001f)]
//    public void CreateChildren_Half(float x, float y, float width, float height)
//    {
//        QuadtreeNode quadtree = new QuadtreeNode(new Rect(x, y, width, height));

//        InvokeCreateChildren(quadtree);

//        List<QuadtreeNode> children = GetChildren(quadtree);

//        Assert.AreEqual(GetArea(children[3]).xMax, GetArea(children[0]).xMin);
//        Assert.AreEqual(GetArea(children[1]).yMax, GetArea(children[0]).yMin);
//    }

//    private void InvokeCreateChildren(QuadtreeNode quadtree)
//    {
//        string createChildrenMathodName = "CreateChildren";

//        MethodInfo method = quadtree.GetType().GetMethod(createChildrenMathodName, BindingFlags.Instance | BindingFlags.NonPublic);

//        method.Invoke(quadtree, new object[0]);
//    }

//    private List<QuadtreeNode> GetChildren(QuadtreeNode quadtree)
//    {
//        string childrenName = "_children";

//        FieldInfo childrenField = quadtree.GetType().GetField(childrenName, BindingFlags.NonPublic | BindingFlags.Instance);

//        return (List<QuadtreeNode>)childrenField.GetValue(quadtree);
//    }

//    private Rect GetArea(QuadtreeNode quadtree)
//    {
//        string fieldName = "_area";

//        FieldInfo childrenField = quadtree.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

//        return (Rect)childrenField.GetValue(quadtree);
//    }

//    /// <summary>
//    /// 测试四叉树创建子节点方法创建的子节点的边缘部分是否和父节点一致，如果不同，则四叉树有可能出现空隙，导致在空隙中的碰撞器无法存入四叉树
//    /// </summary>
//    [Test]
//    [TestCase(1f, 2f, 4f, 3f)]
//    [TestCase(2.0649f, 4.97412f, 264.94125f, 23.64985f)]
//    [TestCase(0.0001f, 0.0001f, 0.001f, 0.0001f)]
//    public void CreateChildren_Edge(float x, float y, float width, float height)
//    {
//        QuadtreeNode quadtree = new QuadtreeNode(new Rect(x, y, width, height));

//        InvokeCreateChildren(quadtree);

//        List<QuadtreeNode> children = GetChildren(quadtree);

//        Assert.AreEqual(x + width, GetArea(children[0]).xMax);
//        Assert.AreEqual(y + height, GetArea(children[0]).yMax);
//        Assert.AreEqual(x, GetArea(children[2]).xMin);
//        Assert.AreEqual(y, GetArea(children[2]).yMin);
//    }
//}
