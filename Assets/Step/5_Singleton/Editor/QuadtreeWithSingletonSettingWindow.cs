/*
 *  QuadtreeWithSingleton的设置文件的编辑窗口
 */

using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using System.Linq;

namespace MtC.Tools.Quadtree.Step.Singleton
{
    public class QuadtreeWithSingletonSettingWindow : EditorWindow //EditorWindow：继承这个类的类可以创建窗口，就像 Inspector、Project 那样
    {
        const string settingObjectName = "QuadtreeWithSingletonSetting";
        QuadtreeSettingSingleton setting
        {
            get
            {
                if (_setting != null)
                    return _setting;

                _setting = GetSettingObject(settingObjectName);
                return _setting;
            }
            //也是单例的
        }
        QuadtreeSettingSingleton _setting;


        [MenuItem("Tools/Quadtree/Step/5-QuadtreeWithSingletonSettingWindow", priority = 5)] //MenuItem：在菜单栏创建一个选项，点击后执行这个方法
        static void GetWindow()
        {
            QuadtreeWithSingletonSettingWindow window = GetWindow<QuadtreeWithSingletonSettingWindow>();
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
            Editor.CreateEditor(setting).DrawDefaultInspector(); //用 CreatEditor 创建设置文件的编辑器，之后用这个编辑器的 DrawDefauleInspector 绘制出默认窗口
        }



        //获取设置文件
        QuadtreeSettingSingleton GetSettingObject(string settingObjectName)
        {
            QuadtreeSettingSingleton settingObject = LoadSetting(settingObjectName);
            if (settingObject != null)
                return settingObject;
            return CreatSettingObject(settingObjectName);
        }

        static QuadtreeSettingSingleton LoadSetting(string settingObjectName)
        {
            return Resources.Load<QuadtreeSettingSingleton>(settingObjectName); //Resources.Load：传入 相对于Resources文件夹 的路径，加载出资源文件，资源文件必须在名字叫“Resources”的文件夹下
            /*
             *  Resources：Unity的一种在运行时加载资源的方式，先把资源文件放到名字叫“Resources”的文件夹下，之后通过 Resources.Load 加载出来
             *  
             *  Unity官方对Resources给出的最佳使用方式是：别用
             *  
             *  Resources里的文件不会和普通文件夹里的文件一起打包压缩后放到游戏文件夹里，而是根据不同平台存在不同的位置，其本身对优化也会造成影响，官方建议在最终发布之前用其他方式替换掉Resources
             *  
             *  详情请看：
             *  https://docs.unity3d.com/ScriptReference/Resources.html
             *  https://unity3d.com/cn/learn/tutorials/topics/best-practices/resources-folder
             */
        }

        QuadtreeSettingSingleton CreatSettingObject(string settingObjectName)
        {
            string settingScriptFilePath = GetSettingScriptFilePath();

            if (!AssetDatabase.IsValidFolder(settingScriptFilePath + "Resources")) //AssetDatabase.IsValidFolder：检测有没有这个路径表示的文件夹
                CreatResourcesFolder(settingScriptFilePath);

            QuadtreeSettingSingleton settingObject = CreateInstance<QuadtreeSettingSingleton>();
            AssetDatabase.CreateAsset(settingObject, settingScriptFilePath + "Resources/" + settingObjectName + ".asset"); //AssetDatabase.CreateAsset：创建资源文件

            return settingObject;
        }
        string GetSettingScriptFilePath()
        {
            QuadtreeSettingSingleton settingInstance = CreateInstance<QuadtreeSettingSingleton>(); //CreateInstance：创建一个继承了ScriptableObject的类的实例
            MonoScript settingScriptFile = MonoScript.FromScriptableObject(settingInstance); //MonoScript.FromScriptableObject：根据ScriptableObject的实例获取到脚本文件
            string fullPath = AssetDatabase.GetAssetPath(settingScriptFile); //AssetDatabase.GetAssetPath：根据文件获取路径，包含了文件名
            string folderPath = fullPath.Substring(0, fullPath.LastIndexOf("/") + 1); //截取这个路径到最后一个斜线，这就是设置文件脚本所在的文件夹的路径
            return folderPath;
        }
        void CreatResourcesFolder(string parentFolderPath)
        {
            if (parentFolderPath.Last() == '/') //Last在 System.Linq 里面的
                parentFolderPath = parentFolderPath.Substring(0, parentFolderPath.Length - 1); //后面一步需要的是到文件夹名字为止，不需要最后斜线的路径，所以如果传入的路径最后一个字符是斜线则截取掉

            AssetDatabase.CreateFolder(parentFolderPath, "Resources"); //AssetDatabase.CreateFolder：在指定路径下创建一个文件夹，这个路径是到文件夹名字为止的
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
}
