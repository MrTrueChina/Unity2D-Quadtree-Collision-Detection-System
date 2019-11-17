using UnityEngine;

namespace MtC.Tools.QuadtreeCollider.Test.API.OnTriggerEnterExecuteTime
{
    public class Loger : MonoBehaviour
    {
        private float _lastTime;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Debug.Log("TriggerEnter，与上一次时间间隔：" + (Time.time - _lastTime));
            _lastTime = Time.time;
        }
    }
}
