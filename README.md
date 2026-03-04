## 项目简介

这是一个 Unity Demo 项目，包含三个独立场景：

- **RTS/RPG 双模式场景**：可切换 RTS 框选控制和 RPG 第三人称控制，包含完整的单位系统、导航、战斗和 UI
- **雨天涟漪场景**：粒子碰撞触发地面涟漪，水面带积水区域、流动扰动和 Fresnel 反射
- **Boids 群体模拟场景**：100+ 个体的分离 / 对齐 / 聚合群体行为

引擎：Unity 2021+  
语言：C#、HLSL

---

## 场景介绍

### 场景1：RTS/RPG 双模式系统

`Assets/Scenes/SampleScene.unity`

**包含什么：**

- **双模式切换**：运行时可在 RTS 和 RPG 两种控制模式间切换
  - RTS 模式：鼠标拖拽框选多个单位，右键地面移动，右键敌人发起攻击
  - RPG 模式：WASD 控制角色移动，鼠标左键攻击
- **单位系统**：每个单位由 `Health` / `Move` / `Attack` / `Animation` / `NavigationAgent` / `StateMachine` 等组件组成，组件可按需挂载或移除
- **阵型系统**：选中多个单位后移动，自动生成六边形阵型并分配槽位
- **导航系统**：NavMesh 路径规划 + RVO 动态避障，多单位移动时互相绕行
- **战斗系统**：扇形范围命中判定，攻击状态机控制攻击节奏，地面圆形光效指示攻击范围
- **UI 系统**：HUD 常驻界面 + Setting 弹窗，支持 HUD / Window / Popup / Overlay 四种层级，Window 和 Popup 以栈管理开关顺序
- **框选渲染**：拖拽时屏幕上显示矩形选框，松开后将选框范围内的单位全部选中

---

### 场景2：雨天涟漪特效

`Assets/Scenes/Rain.unity`

**包含什么：**

- 粒子系统模拟降雨，雨滴落地触发地面涟漪
- 最多 128 个涟漪同时存在，每个涟漪有独立位置和生命周期
- 涟漪以正弦波形式向外扩散，随时间和距离衰减
- 地面带积水区域遮罩（噪声图定义范围）
- 水面有双层流动扰动和 Fresnel 湿润反射

---

### 场景3：Boids 群体模拟

`Assets/Scenes/boids.unity`

**包含什么：**

- 100+ 个方块模拟群体行为
- 每个个体受三种力驱动：分离（避免重叠）、对齐（跟随群体方向）、聚合（靠近群体中心）
- 边界推力防止群体散开
- Inspector 中可实时调节感知半径、各行为权重、最大速度

---

## 运行方式

1. 用 Unity 2021+ 打开项目根目录
2. 在 `Assets/Scenes` 中打开对应场景，点击 Play
   - **RTS/RPG 场景**：RTS 模式用鼠标框选和右键操作；按切换键进入 RPG 模式后用 WASD + 鼠标左键
   - **雨天场景**：自动播放，观察地面涟漪效果
   - **Boids 场景**：自动运行，可在 Inspector 调参观察行为变化

---

## 代码结构

### RTS/RPG 系统
Assets/Scripts/pathFinding/
├── Unit.cs # 单位容器，聚合所有功能组件
├── HealthComponent.cs # 生命值
├── MoveComponent.cs # 移动
├── AttackComponent.cs # 攻击
├── WeaponComponent.cs # 武器挂点
├── AnimationComponent.cs # 动画
├── NavigationAgentComponent.cs # 导航代理（NavMesh + RVO）
├── OrderQueueComponent.cs # 指令队列
├── SensorTargetComponent.cs # 目标感知
├── TeamComponent.cs # 阵营
├── UnitDataComponent.cs # 单位数据（ScriptableObject）
├── UnitStateMachine.cs # 状态机
├── PlayerCtrl.cs # RTS 鼠标控制
├── RPGUnitController.cs # RPG 角色控制
├── GameModeManager.cs # 模式切换管理
├── SelectBox.cs # 框选渲染与单位拾取
├── HexFormationPlanner.cs # 六边形阵型规划
├── RVOManager.cs # RVO 避障管理
├── SpawnManager.cs # 单位生成
├── Enum.cs # 枚举定义（Team / Order / State / UIType 等）
└── UI/
├── UIManager.cs # UI 单例管理器（栈式开关）
├── UIView.cs # UI 基类（OnOpen / OnClose / OnTop 钩子）
├── UIHUD.cs # HUD 界面
└── UISetting.cs # Setting 弹窗