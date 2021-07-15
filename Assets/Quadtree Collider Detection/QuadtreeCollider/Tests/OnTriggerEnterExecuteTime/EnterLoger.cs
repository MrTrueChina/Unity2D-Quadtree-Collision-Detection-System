using UnityEngine;

namespace MtC.Tools.QuadtreeCollider.Test
{
    public class EnterLoger : MonoBehaviour
    {
        private float lastTime;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Debug.Log("TriggerEnter，与上一次时间间隔：" + (Time.time - lastTime));
            lastTime = Time.time;
        }
    }
}
