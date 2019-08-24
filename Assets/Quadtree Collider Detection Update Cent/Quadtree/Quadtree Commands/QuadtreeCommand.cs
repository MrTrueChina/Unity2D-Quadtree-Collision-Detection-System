using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider.UpdateCent
{
    /// <summary>
    /// 四叉树的命令接口
    /// </summary>
    public interface QuadtreeCommand
    {
        void DoCommand(QuadtreeCollider collider);
    }
}
