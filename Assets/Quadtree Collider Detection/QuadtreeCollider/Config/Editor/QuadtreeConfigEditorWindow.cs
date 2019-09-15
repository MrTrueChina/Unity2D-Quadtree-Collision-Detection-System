using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider
{
    public class QuadtreeConfigEditorWindow : EditorWindow
    {
        private QuadtreeConfig config
        {
            get
            {
                if (_config != null)
                    return _config;

                _config = GetSettingObject();
                return _config;
            }
        }
        private QuadtreeConfig _config;

        [MenuItem("Tools/Quadtree/Quadtree Config")]
        private static void GetWindow()
        {
            ((QuadtreeConfigEditorWindow)GetWindow(typeof(QuadtreeConfigEditorWindow))).Show();
        }

        private void OnGUI()
        {
            DrawProposal();
            GUILayout.Space(5);
            DrawSettingEditor();
        }

        private void DrawProposal()
        {
            EditorGUILayout.LabelField("本设置使用了Resources文件夹，对优化有影响，建议在发布前改用其他方式设置（如硬编码或数据类）");
        }

        private void DrawSettingEditor()
        {
            Editor.CreateEditor(config).DrawDefaultInspector();
        }

        QuadtreeConfig GetSettingObject()
        {
            QuadtreeConfig settingObject = LoadSetting();
            if (settingObject != null)
                return settingObject;
            return CreatSettingObject();
        }

        static QuadtreeConfig LoadSetting()
        {
            return Resources.Load<QuadtreeConfig>(QuadtreeConfig.CONFIG_OBJECT_NAME);
        }

        QuadtreeConfig CreatSettingObject()
        {
            string settingScriptFilePath = GetSettingScriptFilePath();

            if (!AssetDatabase.IsValidFolder(settingScriptFilePath + "Resources"))
                CreatResourcesFolder(settingScriptFilePath);

            QuadtreeConfig settingObject = CreateInstance<QuadtreeConfig>();
            AssetDatabase.CreateAsset(settingObject, settingScriptFilePath + "Resources/" + QuadtreeConfig.CONFIG_OBJECT_NAME + ".asset");

            return settingObject;
        }

        string GetSettingScriptFilePath()
        {
            string fullPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(CreateInstance<QuadtreeConfig>()));
            return fullPath.Substring(0, fullPath.LastIndexOf("/") + 1);
        }

        void CreatResourcesFolder(string parentFolderPath)
        {
            if (parentFolderPath.Last() == '/')
                parentFolderPath = parentFolderPath.Substring(0, parentFolderPath.Length - 1);
            AssetDatabase.CreateFolder(parentFolderPath, "Resources");
        }

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

            Vector3 upperRight = new Vector3(QuadtreeConfig.startArea.xMax, QuadtreeConfig.startArea.yMax, 0);
            Vector3 lowerRight = new Vector3(QuadtreeConfig.startArea.xMax, QuadtreeConfig.startArea.yMin, 0);
            Vector3 lowerLeft = new Vector3(QuadtreeConfig.startArea.xMin, QuadtreeConfig.startArea.yMin, 0);
            Vector3 upperLeft = new Vector3(QuadtreeConfig.startArea.xMin, QuadtreeConfig.startArea.yMax, 0);

            Handles.DrawLine(upperRight, lowerRight);
            Handles.DrawLine(lowerRight, lowerLeft);
            Handles.DrawLine(lowerLeft, upperLeft);
            Handles.DrawLine(upperLeft, upperRight);
        }

        [PostProcessBuild(0)]
        static void OnBuild(BuildTarget target, string path)
        {
            if (LoadSetting() != null)
                Debug.LogWarning("检测到 Resources 文件夹中有四叉树设置文件，为游戏优化着想，建议改用其他方式（如硬编码）进行设置，之后移除设置文件、设置脚本文件和设置编辑器脚本文件");
        }
    }
}
