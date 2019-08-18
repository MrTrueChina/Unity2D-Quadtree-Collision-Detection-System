/*
 *  QuadtreeWithSingleton的设置文件的类
 *  
 *  ScriptableObject：简单地说是一个可以创建资源文件实例的类，继承自这个类的类的对象可以作为资源文件保存到本地，资源文件独立于场景之外，与脚本、模型、材质等同属于资源。
 *  
 *  具体请参考：
 *  https://docs.unity3d.com/Manual/class-ScriptableObject.html
 *  https://unity3d.com/cn/learn/tutorials/modules/beginner/live-training-archive/scriptable-objects
 *  https://docs.unity3d.com/ScriptReference/ScriptableObject.html
 */

using UnityEngine;

namespace MtC.Tools.Quadtree.Step.Singleton
{
    public class QuadtreeSettingSingleton : ScriptableObject
    {
        public float top = 1960;
        public float right = 1080;
        public float bottom = 0;
        public float left = 0;
        public int maxLeafsNumber = 5;
        public float minSideLength = 10;
    }
}
