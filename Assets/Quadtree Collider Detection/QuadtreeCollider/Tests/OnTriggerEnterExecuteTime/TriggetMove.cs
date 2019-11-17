using UnityEngine;

namespace MtC.Tools.QuadtreeCollider.Test.API.OnTriggerEnterExecuteTime
{
    public class TriggetMove : MonoBehaviour
    {
        public Vector3 _triggerPosition;
        public Vector3 _outOfTriggerPosition;
        public int _moveTime;

        private void Update()
        {
            for(int i = 0; i < _moveTime; i++)
            {
                transform.position = _triggerPosition;
                transform.position = _outOfTriggerPosition;
            }
        }
    }
}
