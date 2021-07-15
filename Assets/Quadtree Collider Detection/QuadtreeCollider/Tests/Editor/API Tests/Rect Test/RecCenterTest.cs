using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;

namespace MtC.Tools.QuadtreeCollider.Test
{
    [TestFixture]
    public class RecCenterTest
    {
        [Test]
        public void CenterTest()
        {
            Rect rect = new Rect(-1, -1, 2, 2);

            if (rect.center == Vector2.zero)
            {
                Debug.Log("Rect 的 Center 默认返回视觉上的中心，即 x y 各一半");
            }
            else if (rect.center == -Vector2.one)
            {
                Debug.Log("Rect 的 Center 默认返回一个角，即 x,y 各一半");
            }
            else
            {
                Debug.Log("数值为 " + rect.ToString() + " 的 Rect 的 Center 返回是 " + rect.center);
            }
        }
    }
}
