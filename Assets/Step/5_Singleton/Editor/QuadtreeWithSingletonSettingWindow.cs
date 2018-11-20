using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class QuadtreeWithSingletonSettingWindow : EditorWindow
{
    const string settingObjectName = "QuadtreeWithSingletonSetting";
    QuadtreeWithSingletonSetting setting
    {
        get
        {
            if (_setting != null)
                return _setting;

            _setting = GetSettingObject(settingObjectName);
            return _setting;
        }
    }
    QuadtreeWithSingletonSetting _setting;


    [MenuItem("Tool/QuadtreeWithSingletonSettingWindow")]
    static void GetWindow()
    {
        QuadtreeWithSingletonSettingWindow window = (QuadtreeWithSingletonSettingWindow)GetWindow(typeof(QuadtreeWithSingletonSettingWindow));
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
    QuadtreeWithSingletonSetting GetSettingObject(string settingObjectName)
    {
        QuadtreeWithSingletonSetting settingObject = LoadSetting(settingObjectName);
        if (settingObject != null)
            return settingObject;
        return CreatSettingObject(settingObjectName);
    }

    QuadtreeWithSingletonSetting CreatSettingObject(string settingObjectName)
    {
        string settingScriptFilePath = GetSettingScriptFilePath();

        if (!AssetDatabase.IsValidFolder(settingScriptFilePath + "Resources"))
            CreatResourcesFolder();

        QuadtreeWithSingletonSetting settingObject = CreateInstance<QuadtreeWithSingletonSetting>();
        AssetDatabase.CreateAsset(settingObject, settingScriptFilePath + "Resources/" + settingObjectName + ".asset");

        return settingObject;
    }

    string GetSettingScriptFilePath()
    {
        QuadtreeWithSingletonSetting settingInstance = CreateInstance<QuadtreeWithSingletonSetting>();
        MonoScript settingScriptFile = MonoScript.FromScriptableObject(settingInstance);
        string fullPath = AssetDatabase.GetAssetPath(settingScriptFile);
        string folderPath = fullPath.Substring(0, fullPath.LastIndexOf("/") + 1);
        return folderPath;
    }

    static QuadtreeWithSingletonSetting LoadSetting(string settingObjectName)
    {
        return Resources.Load<QuadtreeWithSingletonSetting>(settingObjectName);
    }

    void CreatResourcesFolder()
    {
        string settingScriptFilePath = GetSettingScriptFilePath();
        AssetDatabase.CreateFolder(settingScriptFilePath.Substring(0, settingScriptFilePath.Length - 1), "Resources");
    }



    //绘制范围
    private void OnEnable()
    {
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
    }
    
    void OnSceneGUI(SceneView sceneView)
    {
        //Handles：和Gizmos很像的一个类，用起来也很像Gizimos，但Handles不在 OnDrawGizmos 和 OnDrawGizmosSelected 里也可以使用
        Handles.color = Color.red * 0.9f;
        
        Vector3 upperRight = new Vector3(setting.right, setting.top, 0);
        Vector3 lowerRight = new Vector3(setting.right, setting.bottom, 0);
        Vector3 lowerLeft = new Vector3(setting.left, setting.bottom, 0);
        Vector3 upperLeft = new Vector3(setting.left, setting.top, 0);

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
