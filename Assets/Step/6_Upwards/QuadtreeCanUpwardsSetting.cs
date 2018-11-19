using UnityEngine;

//[CreateAssetMenu(menuName = "Tool/QuadtreeCanUpwardsSetting", fileName = "QuadtreeCanUpwardsSetting")]    //创建设置文件之后就没什么用了
public class QuadtreeCanUpwardsSetting : ScriptableObject
{
    public float top = 1960;
    public float right = 1080;
    public float bottom = 0;
    public float left = 0;
    public int maxLeafsNumber = 5;
    public float minSideLength = 10;
}
