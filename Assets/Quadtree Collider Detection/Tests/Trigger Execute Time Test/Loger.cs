using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider.Test.API.TriggerExecuteTime
{
    public class Loger : MonoBehaviour
    {
        private float _lastTriggerTime;

        private void OnTriggerStay2D(Collider2D collision)
        {
            Debug.Log("发生触发，与上一次触发的间隔为：" + (Time.time - _lastTriggerTime));
            _lastTriggerTime = Time.time;
        }
    }
}
