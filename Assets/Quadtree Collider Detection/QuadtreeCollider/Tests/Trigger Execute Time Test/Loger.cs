using UnityEngine;

namespace MtC.Tools.QuadtreeCollider.Test.API.TriggerExecuteTime
{
    public class Loger : MonoBehaviour
    {
        private float lastTriggerTime;

        private void OnTriggerStay2D(Collider2D collision)
        {
            Debug.Log("发生触发，与上一次触发的间隔为：" + (Time.time - lastTriggerTime));
            lastTriggerTime = Time.time;
        }
    }
}
