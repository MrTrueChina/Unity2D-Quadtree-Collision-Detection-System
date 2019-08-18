using UnityEngine;

namespace MtC.Tools.Quadtree.Step.Upwards
{
    public class QuadtreeSettingUpwards : ScriptableObject
    {
        public float startTop = 1960;
        public float startRight = 1080;
        public float startBottom = 0;
        public float startLeft = 0;
        public int maxLeafsNumber = 5;
        public float minSideLength = 10;
    }
}
