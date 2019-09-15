using UnityEngine;

namespace MtC.Tools.QuadtreeCollider
{
    /// <summary>
    /// 四叉树配置文件
    /// </summary>
    public class QuadtreeConfig : ScriptableObject
    {
        /// <summary>
        /// 四叉树配置资源文件的文件名
        /// </summary>
        public const string CONFIG_OBJECT_NAME = "Quadtree Config";

        private static QuadtreeConfig config
        {
            get
            {
                if (_config != null)
                    return _config;
                else
                    _config = Resources.Load<QuadtreeConfig>(CONFIG_OBJECT_NAME);
                return _config;
            }
        }
        private static QuadtreeConfig _config;

        /// <summary>
        /// 一个节点里的碰撞器数量上限，超过上限后进行分割
        /// </summary>
        public static int maxCollidersNumber
        {
            get { return config._maxCollidersNumber; }
        }
        [SerializeField]
        private int _maxCollidersNumber = 10;

        /// <summary>
        /// 单个节点的最短边的最小长度，当任意一个边的长度小于这个长度时，无论碰撞器数量，不再进行分割
        /// </summary>
        public static float minSideLength
        {
            get { return config._minSideLength; }
        }
        [SerializeField]
        private float _minSideLength = 10; // 这个值用于应对过度分割导致树深度过大性能反而下降的情况，同时可以避免大量碰撞器位置完全相同导致的无限分割

        /// <summary>
        /// 初始根节点范围
        /// </summary>
        public static Rect startArea
        {
            get { return config._startArea; }
        }
        [SerializeField]
        private Rect _startArea = new Rect(-1, -1, 1922, 1082);
    }
}
