using UnityEditor;
using UnityEngine;

public class QuadtreeCanUpwardsSettingWindow : EditorWindow
{
    QuadtreeCanUpwardsSetting setting
    {
        get
        {
            if (_setting != null)
                return _setting;

            _setting = Resources.Load("QuadtreeCanUpwardsSetting") as QuadtreeCanUpwardsSetting;
            return _setting;
        }
    }
    QuadtreeCanUpwardsSetting _setting;


    [MenuItem("Tool/QuadtreeCanUpwardsSettingWindow")]
    static void GetWindow()
    {
        QuadtreeCanUpwardsSettingWindow window = (QuadtreeCanUpwardsSettingWindow)GetWindow(typeof(QuadtreeCanUpwardsSettingWindow));
        window.Show();
    }


    private void OnGUI()
    {
        Editor.CreateEditor(setting).DrawDefaultInspector();
    }


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
}
