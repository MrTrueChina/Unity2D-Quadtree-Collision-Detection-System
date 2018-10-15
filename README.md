# Unity四叉树碰撞检测</br>
四叉树可以类比二叉树，一般是将二维平面一次次做四等分形成的树，常用于二维空间上的物体的操作。</br>
四叉树碰撞检测是四叉树的常见应用之一，因为Unity的碰撞系统是基于物理计算的，成本相当高昂，如果制作弹幕类游戏很难保证高帧数（说的就是你，过气车万）。使用没有物理功能的四叉树进行碰撞检测可以解决这个问题。</br>
</br>
### 文件夹内容：
| 文件夹 | 内容 |
| ------------- |:-------------| 
| Assets/Quadtree | 实用版四叉树碰撞检测脚本 |
| Assets/Quadtree/Editor | 测试代码，用来在Unity的 Test Runnner 窗口做TDD（测试驱动开发），删掉不影响功能 |
| Assets/Quadtree/Example | 实用版四叉树碰撞检测的演示场景和脚本 |
| Assets/Step | 从简单逐步复杂的代码，加了面向新人的大批量注释，如果是刚开始研究四叉树可以按照这个文件夹的顺序阅读 |
| Assets/Step/0_QuadtreeBasic | 最初版的四叉树，碰撞器不能移动也没有半径，就是一个个固定的点，主要用来理解碰撞检测的原理和四叉树基础的核心功能。此外还有大量的新人入门、名词解释等注释 |
| Assets/Step/1_QuadtreeWithRadius | 在0的基础上增加了碰撞器的半径，但碰撞器依然不能移动，半径也不能改变 |
| Assets/Step/2_QuadtreeWithUpdate | 在1的基础上增加更新功能，从这一步开始碰撞器可以移动也可以改变半径了 |
| Assets/Step/3_QuadtreeWithEventDelegate | 在2的基础上增加事件委托，实现类似Unity的 OnCollision 的效果，解释了什么是事件和委托，可以帮助新人理解事件委托，但不保证看完就懂 |
| ProjectSettings | Unity的ProjectSettings文件夹，里面设置了代码执行顺序（重要）和其他各种设置 |
| UnityPackageManager | 说实话我不知道这个文件夹有什么用，但 Github for Unity 把他识别出来了，应该是有用的 |
## 注意：由于没有物理功能，四叉树的碰撞不会像Unity自带碰撞一样互相弹开，而是像触发器一样互相穿过
