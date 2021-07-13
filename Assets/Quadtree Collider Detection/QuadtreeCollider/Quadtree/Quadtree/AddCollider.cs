using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MtC.Tools.QuadtreeCollider
{
    /// <summary>
    /// 四叉树添加碰撞器部分
    /// </summary>
    internal partial class Quadtree : MonoBehaviour
    {
        /// <summary>
        /// 比特索引，将判断根节点位于新根节点的哪个子节点索引的映射表，规则：[上1, 右1]
        /// </summary>
        private static readonly Dictionary<byte, int> indexByteToInt = new Dictionary<byte, int>
        {
            { 0b11, 0 }, // 右上 => 11 => 0
            { 0b01, 1 }, // 右下 => 01 => 1
            { 0b00, 2 }, // 左下 => 00 => 2
            { 0b10, 3 }, // 左上 => 10 => 3
        };

        /// <summary>
        /// 添加碰撞器
        /// </summary>
        /// <param name="collider"></param>
        internal QuadtreeNode.OperationResult DoAddCollider(QuadtreeCollider collider)
        {
            // XXX：只假设了存入失败是因为碰撞器不在范围内，可能需要添加限制，或在每次循环时对四叉树总区域进行判断，如果节点就在总区域内部但还是存入失败则报错

            // FIXME：如果是更新导致的碰撞器重新存入，重新存入导致四叉树生长，则会发生一个问题：新生长的四叉树节点是不会更新到的，这会导致一帧的时间里这些碰撞器无法被检测到碰撞
            // FIXME：这会导致碰撞器明明没离开但还是离开碰撞，下一帧又进入碰撞

            // 存入
            QuadtreeNode.OperationResult result = root.AddColliderByArea(collider);

            // 循环存入碰撞器，直到存入成功
            while (!result.Success)
            {
                // 如果存入失败则说明碰撞器在四叉树外，让四叉树向碰撞器方向生长
                UpwordGroupToCollider(collider);
                
                // 再次存入
                result = root.AddColliderByArea(collider);
            }

            // 更新映射表
            collidersToNodes.OverlayMerge(result.CollidersToNodes).RemoveOnValueIsNull();

            return result;
        }

        /// <summary>
        /// 让四叉树向碰撞器方向生长
        /// </summary>
        /// <param name="collider"></param>
        private void UpwordGroupToCollider(QuadtreeCollider collider)
        {
            // 获取当前根节点在新的四叉树根节点中的位置的比特索引
            byte mainChildIndexByte = GetMainChildIndexByte(collider);

            // 获取新的四叉树根节点的子节点列表
            List<QuadtreeNode> children = GetChildren(mainChildIndexByte);

            // 创建新的四叉树根节点的区域
            Rect newRootArea = new Rect(children[QuadtreeNode.LEFT_BOTTOM_CHILD_INDEX].Area.position, children[QuadtreeNode.LEFT_BOTTOM_CHILD_INDEX].Area.size * 2);

            // 创建新的四叉树节点，并设为新的根节点
            root = new QuadtreeNode(newRootArea, children, indexByteToInt[mainChildIndexByte]);
        }

        /// <summary>
        /// 获取当前根节点在新的四叉树中的子节点位置的比特索引
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        private byte GetMainChildIndexByte(QuadtreeCollider collider)
        {
            // 默认左下角
            byte indexByte = 0b00;

            // 碰撞器位于现有区域原点的左边 => 四叉树向左生长 => 根节点是新根节点右边的节点 => 判断左右的右侧位设为1
            if (collider.Position.x < root.Area.x)
            {
                indexByte |= 0b01;
            }

            // 碰撞器位于现有区域原点的下边 => 四叉树向下生长 => 根节点是新根节点上边的节点 => 判断上下的上方向位设为1
            if (collider.Position.y < root.Area.y)
            {
                indexByte |= 0b10;
            }

            return indexByte;
        }

        /// <summary>
        /// 获取生长后的四叉树的子节点列表
        /// </summary>
        /// <param name="mainChildIndexByte"></param>
        /// <returns></returns>
        private List<QuadtreeNode> GetChildren(byte mainChildIndexByte)
        {
            // 获取生长后的四叉树所需的 xy 轴的 最小、中间、最大 坐标
            GetX(mainChildIndexByte, out float xMin, out float xMiddle, out float xMax);
            GetY(mainChildIndexByte, out float yMin, out float yMiddle, out float yMax);

            List<QuadtreeNode> children = new List<QuadtreeNode>();

            // 添加子节点，如果是当前根节点的位置则设为当前根节点，否则创建新节点
            children.Add(indexByteToInt[mainChildIndexByte] == 0 ? root : new QuadtreeNode(new Rect(xMiddle, yMiddle, root.Area.width, root.Area.height)));
            children.Add(indexByteToInt[mainChildIndexByte] == 1 ? root : new QuadtreeNode(new Rect(xMiddle, yMin, root.Area.width, root.Area.height)));
            children.Add(indexByteToInt[mainChildIndexByte] == 2 ? root : new QuadtreeNode(new Rect(xMin, yMin, root.Area.width, root.Area.height)));
            children.Add(indexByteToInt[mainChildIndexByte] == 3 ? root : new QuadtreeNode(new Rect(xMin, yMiddle, root.Area.width, root.Area.height)));

            return children;
        }

        /// <summary>
        /// 获取新的四叉树的 x 轴 左、中、右 三个坐标
        /// </summary>
        /// <param name="mainChildIndexByte"></param>
        /// <param name="xMin"></param>
        /// <param name="xMiddle"></param>
        /// <param name="xMax"></param>
        private void GetX(byte mainChildIndexByte, out float xMin, out float xMiddle, out float xMax)
        {
            if (IsGrowToRight(mainChildIndexByte))
            {
                xMin = root.Area.x;
                xMiddle = root.Area.xMax;
                xMax = root.Area.xMax + root.Area.width;
            }
            else
            {
                xMin = root.Area.x - root.Area.width;
                xMiddle = root.Area.x;
                xMax = root.Area.xMax;
            }
        }

        /// <summary>
        /// 检测四叉树是否向右生长
        /// </summary>
        /// <param name="mainChildIndexByte"></param>
        /// <returns></returns>
        bool IsGrowToRight(byte mainChildIndexByte)
        {
            return (mainChildIndexByte & 0b01) == 0;
        }

        /// <summary>
        /// 获取新的四叉树的 y 轴 底、中、顶 三个坐标
        /// </summary>
        /// <param name="mainChildIndexByte"></param>
        /// <param name="yMin"></param>
        /// <param name="yMiddle"></param>
        /// <param name="yMax"></param>
        private void GetY(byte mainChildIndexByte, out float yMin, out float yMiddle, out float yMax)
        {
            if (IsGrowToUp(mainChildIndexByte))
            {
                yMin = root.Area.y;
                yMiddle = root.Area.yMax;
                yMax = root.Area.yMax + root.Area.height;
            }
            else
            {
                yMin = root.Area.y - root.Area.height;
                yMiddle = root.Area.y;
                yMax = root.Area.yMax;
            }
        }

        /// <summary>
        /// 检测四叉树是否向上生长
        /// </summary>
        /// <param name="mainChildIndexByte"></param>
        /// <returns></returns>
        bool IsGrowToUp(byte mainChildIndexByte)
        {
            return (mainChildIndexByte & 0b10) == 0;
        }
    }
}
