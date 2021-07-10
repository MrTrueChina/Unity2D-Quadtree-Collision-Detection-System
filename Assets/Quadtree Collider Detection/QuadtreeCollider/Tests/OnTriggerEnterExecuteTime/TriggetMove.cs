using UnityEngine;

namespace MtC.Tools.QuadtreeCollider.Test.API.OnTriggerEnterExecuteTime
{
    public class TriggetMove : MonoBehaviour
    {
        public Vector3 triggerPosition;
        public Vector3 outOfTriggerPosition;
        public int moveTime;

        private void Update()
        {
            for(int i = 0; i < moveTime; i++)
            {
                transform.position = triggerPosition;
                transform.position = outOfTriggerPosition;
            }
        }
    }
}
