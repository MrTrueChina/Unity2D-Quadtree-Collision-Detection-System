using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MtC.Tools.QuadtreeCollider
{
    /// <summary>
    /// 四叉树碰撞器的Inspector面板GUI绘制Editor
    /// </summary>
    [CustomEditor(typeof(QuadtreeCollider), true)]
    [CanEditMultipleObjects]
    public class QuadtreeColliderEditor : Editor
    {
        private const string AUTO_SUBSCRIBE_WARNING_TEXT = "自动订阅功能只能用于从对象实例化时就挂载的实现了四叉树碰撞事件接口的组件，这是因为自动订阅的碰撞器在 Awake 时遍历所有组件并进行唯一一次订阅，并且不会自动取消订阅。在对象实例化后挂载的组件不会自动订阅。虽然不会自动取消订阅，但由于 UnityEvent 弱引用的特性不会导致内存泄漏。";

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (((QuadtreeCollider)target).AutoSubscribe)
                EditorGUILayout.HelpBox(AUTO_SUBSCRIBE_WARNING_TEXT, MessageType.Warning);
        }
    }
}
