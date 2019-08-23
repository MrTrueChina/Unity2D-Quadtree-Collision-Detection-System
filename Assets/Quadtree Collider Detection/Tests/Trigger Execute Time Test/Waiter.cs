using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider.Test.API.TriggerExecuteTime
{
    /// <summary>
    /// 通过等待时间的循环达到降低Update频率的效果的脚本
    /// </summary>
    public class Waiter : MonoBehaviour
    {
        void Update()
        {
            float time = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - time < 0.1f) ;
        }
    }
}
