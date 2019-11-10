using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace MtC.Tools.Quadtree.Example.Step6Upwards
{
    public class QuadtreeSettingWindowUpwards : EditorWindow
    {
        const string settingObjectName = "QuadtreeCanUpwardsSetting";

        QuadtreeSettingUpwards setting
        {
            get
            {
                if (_setting != null)
                    return _setting;

                _setting = GetSettingObject(settingObjectName);
                return _setting;
            }
        }
        QuadtreeSettingUpwards _setting;

        [MenuItem("Tools/Quadtree/Step/6-QuadtreeCanUpwardsSettingWindow", priority = 6)]
        static void GetWindow()
        {
            QuadtreeSettingWindowUpwards window = (QuadtreeSettingWindowUpwards)GetWindow(typeof(QuadtreeSettingWindowUpwards));
            window.minSize = new Vector2(Screen.width / 3.7f, Screen.width / 12);
            window.Show();
        }

        private void OnGUI()
        {
            DrawProposal();
            GUILayout.Space(5);
            DrawSettingEditor();
        }

        void DrawProposal()
        {
            EditorGUILayout.LabelField("本设置使用了Resources文件夹，对优化有影响，建议在发布前改用其他方式设置（如硬编码或数据类）");
        }

        void DrawSettingEditor()
        {
            Editor.CreateEditor(setting).DrawDefaultInspector();
        }

        //获取设置文件
        QuadtreeSettingUpwards GetSettingObject(string settingObjectName)
        {
            QuadtreeSettingUpwards settingObject = LoadSetting(settingObjectName);
            if (settingObject != null)
                return settingObject;
            return CreatSettingObject(settingObjectName);
        }

        static QuadtreeSettingUpwards LoadSetting(string settingObjectName)
        {
            return Resources.Load<QuadtreeSettingUpwards>(settingObjectName);
        }

        QuadtreeSettingUpwards CreatSettingObject(string settingObjectName)
        {
            string settingScriptFilePath = GetSettingScriptFilePath();

            if (!AssetDatabase.IsValidFolder(settingScriptFilePath + "Resources"))
                CreatResourcesFolder(settingScriptFilePath);

            QuadtreeSettingUpwards settingObject = CreateInstance<QuadtreeSettingUpwards>();
            AssetDatabase.CreateAsset(settingObject, settingScriptFilePath + "Resources/" + settingObjectName + ".asset");

            return settingObject;
        }

        string GetSettingScriptFilePath()
        {
            string fullPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(CreateInstance<QuadtreeSettingUpwards>()));
            return fullPath.Substring(0, fullPath.LastIndexOf("/") + 1);
        }

        void CreatResourcesFolder(string parentFolderPath)
        {
            if (parentFolderPath.Last() == '/')
                parentFolderPath = parentFolderPath.Substring(0, parentFolderPath.Length - 1);
            AssetDatabase.CreateFolder(parentFolderPath, "Resources");
        }

        //绘制范围
        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        void OnSceneGUI(SceneView sceneView)
        {
            Handles.color = Color.red * 0.9f;

            Vector3 upperRight = new Vector3(setting.startRight, setting.startTop, 0);
            Vector3 lowerRight = new Vector3(setting.startRight, setting.startBottom, 0);
            Vector3 lowerLeft = new Vector3(setting.startLeft, setting.startBottom, 0);
            Vector3 upperLeft = new Vector3(setting.startLeft, setting.startTop, 0);

            Handles.DrawLine(upperRight, lowerRight);
            Handles.DrawLine(lowerRight, lowerLeft);
            Handles.DrawLine(lowerLeft, upperLeft);
            Handles.DrawLine(upperLeft, upperRight);
        }

        //发布时提示
        [PostProcessBuild(0)]
        static void OnBuild(BuildTarget target, string path)
        {
            if (LoadSetting(settingObjectName) != null)
                Debug.LogWarning("检测到 Resources 文件夹中有四叉树设置文件，为游戏优化着想，建议改用其他方式（如硬编码）进行设置，之后移除设置文件、设置脚本文件和设置编辑器脚本文件");
        }
    }
}
