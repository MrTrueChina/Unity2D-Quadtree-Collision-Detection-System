using UnityEngine;

namespace MtC.Tools.Quadtree.Old
{
    public class QuadtreeSetting : ScriptableObject
    {
        public float startTop = 1960;
        public float startRight = 1080;
        public float startBottom = 0;
        public float startLeft = 0;
        public int maxLeafsNumber = 5;
        public float minSideLength = 10;
    }
}