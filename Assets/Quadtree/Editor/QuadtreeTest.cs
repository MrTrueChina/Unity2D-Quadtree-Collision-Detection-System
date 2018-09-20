using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class QuadtreeTest
{
    [Test]
    public void SetLeaf_Normal()
    {
        Quadtree<int> quadtree = new Quadtree<int>(0, 0, 5, 10);

        Vector2[] positions = new Vector2[]
            {
                new Vector2(1,9),
                new Vector2(2,1),
                new Vector2(1,3),
                new Vector2(1.1f,1),
                new Vector2(4,6.2f),
                new Vector2(3.2657f,1),
                new Vector2(1,1),
                new Vector2(2,7),
            };

        for (int i = 0; i < positions.Length; i++)
            Assert.IsTrue(quadtree.SetLeaf(new QuadtreeLeaf<int>(i, positions[i], 1)));
    }
    
    [Test]
    public void SetLeaf_OutOfRect()
    {
        Quadtree<int> quadtree = new Quadtree<int>(0, 0, 5, 10);

        Vector2[] positions = new Vector2[]
            {
                new Vector2(-1,5),
                new Vector2(6,5),
                new Vector2(1,-1),
                new Vector2(1,11),
                new Vector2(6,11),
                new Vector2(6,-1),
                new Vector2(-1,-1),
                new Vector2(-1,11)
            };

        for (int i = 0; i < positions.Length; i++)
            Assert.IsFalse(quadtree.SetLeaf(new QuadtreeLeaf<int>(i, positions[i], 1)));
    }
    
    [Test]
    public void SetLeaf_OnTheEdge()
    {
        Quadtree<int> quadtree = new Quadtree<int>(0, 0, 5, 10);

        Vector2[] positions = new Vector2[]
            {
                new Vector2(1,10),
                new Vector2(5,1),
                new Vector2(1,0),
                new Vector2(0,1),
                new Vector2(5,10),
                new Vector2(5,0),
                new Vector2(0,0),
                new Vector2(0,10),
            };

        for (int i = 0; i < positions.Length; i++)
            Assert.IsTrue(quadtree.SetLeaf(new QuadtreeLeaf<int>(i, positions[i], 1)));
    }


    [Test]
    public void Split_OnTheGap()
    {
        Quadtree<int> quadtree = new Quadtree<int>(0, 0, 5, 10, 3, 0.0002f, 0.0002f);

        quadtree.SetLeaf(new QuadtreeLeaf<int>(0, new Vector2(2.5f / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2, 5f / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2),1));
        quadtree.SetLeaf(new QuadtreeLeaf<int>(0, new Vector2(2.5f / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2, 5f / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2),1));
        quadtree.SetLeaf(new QuadtreeLeaf<int>(0, new Vector2(2.5f / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2, 5f / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2),1));
        quadtree.SetLeaf(new QuadtreeLeaf<int>(0, new Vector2(2.5f / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2, 5f / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2 / 2),1));
    }


    [Test]
    public void CheckCollision_Normal()
    {
        Quadtree<Vector2> quadtree = new Quadtree<Vector2>(0, 0, 5, 10, 3);

        Vector2[] positions = new Vector2[]
            {
                new Vector2(0,0),
                new Vector2(0,5),
                new Vector2(2.5f,5),
                new Vector2(5,10),
            };
        foreach (Vector2 position in positions)
            quadtree.SetLeaf(new QuadtreeLeaf<Vector2>(position, position, 0));

        Assert.AreEqual(1, quadtree.CheckCollision(new Vector2(0, 0), 1).Length);       //检测（0,0），半径1，应有一个：（0,0）
        Assert.AreEqual(2, quadtree.CheckCollision(new Vector2(0, 2.5f), 2.5f).Length); //检测（0,2.5），半径2.5，应有两个：（0,0）（0,5）
        Assert.AreEqual(2, quadtree.CheckCollision(new Vector2(2.5f, 5f), 5).Length);   //检测（2.5,5），半径5，应有两个：（0,5）（2.5,5）
        Assert.AreEqual(4, quadtree.CheckCollision(new Vector2(2.5f, 5f), 10).Length);  //检测（2.5,5），半径10，应全中，4个
    }


    [Test]
    public void RemoveLeaf_Normal()
    {
        Quadtree<int> quadtree = new Quadtree<int>(0, 0, 5, 10);

        QuadtreeLeaf<int> leaf1 = new QuadtreeLeaf<int>(1, new Vector2(2, 2), 1);
        quadtree.SetLeaf(leaf1);

        Assert.IsTrue(quadtree.RemoveLeaf(leaf1));
        Assert.IsFalse(quadtree.RemoveLeaf(leaf1));
    }
}
