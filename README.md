# U2D四叉树碰撞检测系统
[![996.icu](https://img.shields.io/badge/link-996.icu-red.svg)](https://996.icu)
[![LICENSE](https://img.shields.io/badge/license-Anti%20996-blue.svg)](https://github.com/996icu/996.ICU/blob/master/LICENSE)
## 简介：
Unity引擎自带一套基于物理引擎的2D碰撞检测系统，这套系统具有完备的物理功能但成本高昂无法大量使用。  
本系统采用正好相反的思路，在不使用物理引擎的基础上使用传统的四叉树实现一套只具有核心功能但速度极快的碰撞检测系统。  
本系统由一套碰撞器、三个接口、一个设置窗口、一个配置文件组成。  
#### 注意：由于没有物理功能，四叉树的碰撞不会像Unity自带碰撞一样互相弹开，而是像触发器一样互相穿过
****
## 快速开始：
由于U3D没有提供组件挂载和移除事件，本系统基于事件委托进行使用，为简化使用提供了自动订阅功能，因此有两种使用方法：  
### 使用自动订阅：
1.将碰撞器挂载到需要检测碰撞的物体上  
2.在需要进行检测的物体的碰撞器组件上勾选 IsDetector  
3.在需要接收事件的脚本中根据需要实现<a href="#Interface">三个接口</a>中的一个或多个  

**警告**：自动订阅的原理是在 Awake 时查询物体上所有组件并将实现了接口的组件的方法进行**唯一一次订阅**，这个订阅**不会被取消**。因此自动订阅只适用于需要碰撞检测的组件在物体**实例化时就已经存在**的情况（**可以启用和禁用**），如果**组件在实例化后才挂载会无法订阅**。虽然订阅不会取消，但由于使用了 UnityEvent 如果组件中途销毁并不会导致内存泄漏。
### 手动订阅：
1.将碰撞器挂载到需要检测碰撞的物体上  
2.取消勾选 Auto Subscribe  
3.在需要进行检测的物体的碰撞器组件上勾选 IsDetector  
4.根据逻辑在需要的位置使用碰撞器的<a href="#QuadtreeCollider">订阅和取消订阅方法</a>来订阅和取消订阅碰撞器组件的三个事件中的一个或多个  
****
## 配置：
通过 Tools -> Quadtree -> Quadtree Config 的配置窗口可以根据需要调整四叉树参数进行优化。
****
## 完整使用手册：
## namespace： MtC.Tools.Quadtree
### 组件：
#### CircleQuadtreeCollider
圆形碰撞器
<br><br>
### 窗口：
#### Tools -> Quadtree -> Quadtree Config
配置窗口，在这个窗口中进行配置修改
<br><br>
### <a name="Interface">接口</a>：
```C#
public interface IOnQuadtreeCollisionEnter
```
实现接口中的 OnQuadtreeCollisionEnter(QuadtreeCollider collider) 方法，当有碰撞器进入该碰撞器碰撞范围时该方法将被调用
<br><br>
```C#
public interface IOnQuadtreeCollisionStay
```
实现接口中的 OnQuadtreeCollisionStay(QuadtreeCollider collider) 方法，当有碰撞器停留在该碰撞器碰撞范围内时该方法将被调用
<br><br>
```C#
public interface IOnQuadtreeCollisionExit
```
实现接口中的 OnQuadtreeCollisionExit(QuadtreeCollider collider) 方法，当有碰撞器离开该碰撞器碰撞范围时该方法将被调用
### 类：
#### <a name="QuadtreeCollider">QuadtreeCollider：</a>
```C#
public bool IsCollitionToCollider(QuadtreeCollider collider);
如果这个碰撞器与指定碰撞器发生碰撞，返回true，否则返回false

public void SubscribeCollisionEnter(IOnQuadtreeCollisionEnter subscriber);
订阅这个碰撞器的碰撞器进入事件

public void CancelSubscribeCollisionEnter(IOnQuadtreeCollisionEnter subscriber);
取消订阅这个碰撞器的碰撞器进入事件

public void SubscribeCollisionStay(IOnQuadtreeCollisionStay subscriber);
订阅这个碰撞器的碰撞器停留事件

public void CancelSubscribeCollisionStay(IOnQuadtreeCollisionStay subscriber);
取消订阅这个碰撞器的碰撞器停留事件

public void SubscribeCollisionExit(IOnQuadtreeCollisionExit subscriber);
订阅这个碰撞器的碰撞器离开事件

public void CancelSubscribeCollisionExit(IOnQuadtreeCollisionExit subscriber);
取消订阅这个碰撞器的碰撞器离开事件
```
