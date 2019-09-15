using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtC.Tools.QuadtreeCollider
{
    // 添加碰撞器部分
    internal partial class Quadtree : MonoBehaviour
    {
        /// <summary>
        /// 将判断根节点位于新根节点的哪个子节点索引的映射表，规则：[上1, 右1]
        /// </summary>
        private static readonly Dictionary<byte, int> indexByteToInt = new Dictionary<byte, int>
        {
            { 0b11, 0 }, // 右上 => 11 => 0
            { 0b01, 1 }, // 右下 => 01 => 1
            { 0b00, 2 }, // 左下 => 00 => 2
            { 0b10, 3 }, // 左上 => 10 => 3
        };

        internal void DoAddCollider(QuadtreeCollider collider)
        {
            while (!_root.AddCollider(collider)) //TODO：此处假设存入碰撞器失败的原因只会是碰撞器不在四叉树范围中，如果出现存入错误可能就是因为这里出了问题
                UpwordGroupToCollider(collider);
        }

        private void UpwordGroupToCollider(QuadtreeCollider collider)
        {
            byte mainChildIndexByte = GetMainChildIndexByte(collider);

            List<QuadtreeNode> children = GetChildren(mainChildIndexByte);

            Rect newRootArea = new Rect(children[QuadtreeNode.LEFT_BOTTOM_CHILD_INDEX].area.position, children[QuadtreeNode.LEFT_BOTTOM_CHILD_INDEX].area.size * 2);

            _root = new QuadtreeNode(newRootArea, children, indexByteToInt[mainChildIndexByte]);
        }

        private byte GetMainChildIndexByte(QuadtreeCollider collider)
        {
            byte indexByte = 0b00;

            if (collider.position.x < _root.area.x)
                indexByte |= 0b01; // 碰撞器位于现有区域原点的左边 => 四叉树向左生长 => 根节点是新根节点右边的节点 => 判断左右的右侧位设为1
            if (collider.position.y < _root.area.y)
                indexByte |= 0b10;

            return indexByte;
        }

        private List<QuadtreeNode> GetChildren(byte mainChildIndexByte)
        {
            GetX(mainChildIndexByte, out float xMin, out float xMiddle, out float xMax);
            GetY(mainChildIndexByte, out float yMin, out float yMiddle, out float yMax);

            List<QuadtreeNode> children = new List<QuadtreeNode>();

            children.Add(indexByteToInt[mainChildIndexByte] == 0 ? _root : new QuadtreeNode(new Rect(xMiddle, yMiddle, _root.area.width, _root.area.height)));
            children.Add(indexByteToInt[mainChildIndexByte] == 1 ? _root : new QuadtreeNode(new Rect(xMiddle, yMin, _root.area.width, _root.area.height)));
            children.Add(indexByteToInt[mainChildIndexByte] == 2 ? _root : new QuadtreeNode(new Rect(xMin, yMin, _root.area.width, _root.area.height)));
            children.Add(indexByteToInt[mainChildIndexByte] == 3 ? _root : new QuadtreeNode(new Rect(xMin, yMiddle, _root.area.width, _root.area.height)));

            return children;
        }

        private void GetX(byte mainChildIndexByte, out float xMin, out float xMiddle, out float xMax)
        {
            if (IsGrowToRight(mainChildIndexByte))
            {
                xMin = _root.area.x;
                xMiddle = _root.area.xMax;
                xMax = _root.area.xMax + _root.area.width;
            }
            else
            {
                xMin = _root.area.x - _root.area.width;
                xMiddle = _root.area.x;
                xMax = _root.area.xMax;
            }
        }

        bool IsGrowToRight(byte mainChildIndexByte)
        {
            return (mainChildIndexByte & 0b01) == 0;
        }

        private void GetY(byte mainChildIndexByte, out float yMin, out float yMiddle, out float yMax)
        {
            if (IsGrowToUp(mainChildIndexByte))
            {
                yMin = _root.area.y;
                yMiddle = _root.area.yMax;
                yMax = _root.area.yMax + _root.area.height;
            }
            else
            {
                yMin = _root.area.y - _root.area.height;
                yMiddle = _root.area.y;
                yMax = _root.area.yMax;
            }
        }

        bool IsGrowToUp(byte mainChildIndexByte)
        {
            return (mainChildIndexByte & 0b10) == 0;
        }
    }
}
