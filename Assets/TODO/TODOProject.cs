﻿//TODO：这个cs文件用于记录需要写的四叉树
/*
 *  重写旧版四叉树：
 *    节点：
 *      即时存入 - 写完未测试
 *      即时移除 - 写完未测试
 *      存入时分割 - 写完未测试
 *      位置统一更新 - 写完未测试
 *      半径统一更新 - 写完未测试
 *    包装类：
 *      存入时检测是否在范围内，不在则反向生长 - 未开发
 *      包装类保存检测器列表，每帧更新后遍历检测 - 未开发
 *    执行顺序：
 *      包装类比平均慢 - 已设置
 */

/*
 *  带合并四叉树：
 *    节点：
 *      即时存入
 *      即时移除
 *      存入时分割
 *      移除时合并
 *      位置统一更新
 *      半径统一更新
 *    包装类：
 *      存入时检测是否在范围内，不在则反向生长
 *      包装类保存检测器列表，每帧更新后遍历检测
 *    节电池：
 *      创建节点时从池中取节点
 *      合并时向池中存节点
 */

/*
 *  碰撞器-节点字典树
 *  即时存入
 *  即时移除
 *  存入时分割
 *  移除时合并
 *  位置统一更新
 *  半径统一更新
 *  
 *  封装类使用<碰撞器-节点>字典保存碰撞器和节点的关系
 *  存入时返回存入的碰撞器
 *  移除时直接通过碰撞器获取节点，从节点直接移除
 */

/*
 *  全针对操作树：
 *  即时存入
 *  即时移除
 *  存入时分割
 *  移除时合并
 *  包装类调用碰撞器进行更新
 *  位置根据碰撞器位置是否变化更新
 *  半径根据碰撞器半径是否变化更新
 *  
 *  封装类使用<碰撞器-节点>字典保存碰撞器和节点的关系
 *  存入时返回存入的碰撞器
 *  移除时直接通过碰撞器获取节点，从节点直接移除
 *  位置更新直接通过碰撞器获取节点检测是否出界
 *  
 *  碰撞器保存位置和半径，根据位置半径是否变化进行更新
 */
