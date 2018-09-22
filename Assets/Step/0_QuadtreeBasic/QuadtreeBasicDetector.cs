/*
 *  四叉树检测脚本，仅有显示发生碰撞的功能。
 *  
 *  使用方式是挂载到需要检测碰撞的物体上，之后开始运行，在 Scene 面板里拖动物体，如果发生了碰撞的话在 Console 面板里会看到碰撞信息。
 */

using UnityEngine;


public class QuadtreeBasicDetector : MonoBehaviour
{
    [SerializeField]        //关于[SerializeField]请看 QuadtreeBasicObject
    float _radius;

    private void Update()
    {
        GameObject[] objs = QuadtreeBasicObject.CheckCollision(transform.position, _radius);
        
        foreach (GameObject obj in objs)
            Debug.Log("检测到碰撞器 " + obj.name + " ，位置在 " + obj.transform.position);
        /*
         *  假设你不会用foreach，它的格式是：
         *  foreach(类型 变量名A in 同类型的数组或List或其他集合类的变量B)
         *  {
         *      这个大括号会遍历那个变量B里的每个元素，每次遍历都会用前面的变量名A，可以理解为一个简写版的for循环
         *      
         *      假设你连“遍历”这个词都不知道，遍历的意思是从头到尾把每个元素都过一遍
         *  }
         */
    }


    //关于 OnDrawGizmo 请看 QuadtreeBasicObject
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow * 0.8f;
        MyGizmos.DrawCircle(transform.position, _radius, 60);   //Mygizmos是一个自写的类，位置在 QuadtreeCollider 里，这个方法是画圆圈的
    }
}
