# Unity四叉树碰撞检测</br>
四叉树可以类比二叉树，一般是将二维平面一次次做四等分形成的树，常用于二维空间上的物体的操作。</br>
四叉树碰撞检测是四叉树的常见应用之一，因为Unity的碰撞系统是基于物理计算的，成本相当高昂，如果制作弹幕类游戏很难保证高帧数（说的就是你，过气车万）。使用没有物理功能的四叉树进行碰撞检测可以解决这个问题。</br>
</br>
### 使用方法：
1.将 QuadtreeCollider 挂载到需要检测碰撞的物体上。</br>
2.using MtC.Tools.Quadtree;</br>
3.订阅碰撞器的 public Action&lt;GameObject&gt; collisionEvent 事件。</br>
</br>
可以通过 Tools -> Quadtree -> Quadtree Setting 根据需要调整四叉树参数进行优化。</br>
</br>
### 文件夹内容：
| 文件夹 | 内容 |
| ------------- |:-------------| 
| Assets/Quadtree | 实用版四叉树碰撞检测脚本 |
| Assets/Example | 实用版四叉树碰撞检测的演示场景和脚本 |
| Quadtree.unitypackage | 实用版四叉树碰撞检测的资源包 |
| Assets/Step | 从简单逐步复杂的代码，加了面向新人的大批量注释，如果是刚开始研究四叉树可以按照这个文件夹的顺序阅读 |
| Assets/Step/0_Basic | 最初版的四叉树，碰撞器不能移动也没有半径，就是一个个固定的点，主要用来理解碰撞检测的原理和四叉树基础的核心功能。此外还有大量的新人入门、名词解释等注释 |
| Assets/Step/1_Radius | 在0的基础上增加了碰撞器的半径，但碰撞器依然不能移动，半径也不能改变 |
| Assets/Step/2_Update | 在1的基础上增加更新功能，从这一步开始碰撞器可以移动也可以改变半径了 |
| Assets/Step/3.0_Event | 在2的基础上增加事件委托，实现类似Unity的 OnCollision 的效果，解释了什么是事件和委托，可以帮助新人理解事件委托，但不保证看完就懂 |
| Assets/Step/3.1_Action | 在3.0的基础上用 Action 代替了手写的委托，并介绍了 Action 和 Func |
| Assets/Step/4_NestedClass | 在3.1的基础上把 Leaf 和 Field 改成了 Quadtree 的内部类 |
| Asstes/Step/5_Singleton | 在4的基础上把Quadtree和QuadtreeObject合为一个脚本文件，用单例模式自动创建四叉树物体，用ScriptableObject和EditorWindow进行设置。从这一步开始不需要设置脚本执行顺序，也不需要手动创建四叉树物体 |
| Assets/Step/6_Upwards | 在5的基础上增加向上生长的功能，如果叶子存入时位置在四叉树范围以外，四叉树会自动向叶子方向生长以接住叶子 |
| ProjectSettings | Unity的ProjectSettings文件夹，里面是各种设置 |
## 注意：由于没有物理功能，四叉树的碰撞不会像Unity自带碰撞一样互相弹开，而是像触发器一样互相穿过
