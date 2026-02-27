## 项目简介

这是一个基于 Unity 的 **3D RPG/RTS 混合原型项目**，核心目标是练习并展示开放世界类游戏中常见的**单位架构、战斗系统、状态机、导航编队，以及战斗表现（Shader + World UI）**等能力。  
项目更偏「**可讲解的工程架构 Demo**」，而不是内容量很大的完整游戏。

- 引擎：Unity（建议 2021+）
- 语言：C#（运行时）、HLSL（Shader）

---

## 运行方式

1. 使用 Unity 打开项目根目录（包含 `Assets/`、`ProjectSettings/` 的文件夹）。
2. 在 `Assets/Scenes` 中打开示例场景（例如：`Main` / `Test`，以工程当前场景为准）。
3. 进入 Play：
   - **RTS 模式**：使用鼠标选择/右键地面移动单位，右键敌人进行追击与攻击。
   - **RPG 模式**：控制主角移动与攻击，观察攻击范围扫光 + 命中判定 + 受击反馈。

> 具体按键/操作可在 `PlayerCtrl`、`RPGUnitController` 中查看和调整。

---

## 主要特性

- **组件化单位系统（Unit Composition）**
  - `Unit` 作为容器，组合：
    - `HealthComponent`：生命与伤害处理
    - `MoveComponent`：位移与追击
    - `AttackComponent`：RTS/RPG 两套攻击逻辑
    - `AnimationComponent`：统一封装 Animator 播放与状态查询
    - `UnitStateMachine`：RPG 状态机（Idle / Move / Attack / Skill / Dash / Roll）
    - `NavigationAgentComponent`：导航与寻路
    - `UnitDataComponent`：基础数值（HP、攻击、速度、阵营等）

- **战斗系统（Combat System）**
  - 近战追击 + 攻击冷却 + 朝向旋转。
  - RPG 模式下通过 `UnitStateMachine` 与动画时长控制攻击锁定时间。
  - 命中判定：
    - 使用 `Physics.OverlapSphere` 粗筛范围内单位。
    - 使用与 `transform.forward` 的夹角判断是否在扇形攻击区域内。
  - 受击反应通过 `ReactionManager` / `UnitReactionConfig` 统一管理。

- **攻击范围可视化（Attack Range + Shader）**
  - 自定义 Shader：支持**扇形 / 圆形 / 矩形**攻击范围与基于时间的扫光效果。
  - `AttackRangeIndicator`：
    - 使用 `MaterialPropertyBlock` 为每个单位单独设置 `_AttackRangeType`、半径/角度/宽高、`_FillProgress` 等参数，避免共享材质联动。
    - 与攻击动画时长联动，攻击期间显示范围扫光，扫满后应用真实伤害判定。

- **状态机与模式系统（State & GameMode）**
  - `RPGUnitController + UnitStateMachine`：
    - 收集一帧输入为 `RPGInput`（移动轴、攻击输入、鼠标地面点击位置等）。
    - 根据输入与环境状态切换 `RPGState`，驱动行为与动画。
  - `GameMode` 抽象 + `GameModeManager`：
    - `RTSMode` / `RPGMode` 共存，可在同一项目内切换玩法。
    - 通过 `GameModeManager.Attack(unit, target)` 统一攻击入口，根据当前模式调用不同实现。

- **导航与编队（Navigation & Formation）**
  - `HexFormationPlanner`：生成六边形队形插槽。
  - `NavigationAgentComponent` / `RVOManager`：基础寻路与碰撞规避。
  - `OrderQueueComponent`：管理单位的命令队列（Move / Attack / Stop）。

- **数据与扩展**
  - `UnitData` / `UnitDataComponent`：HP、攻击、速度、等级、阵营等基础属性。
  - `UnitRegistry` / `SpawnManager`：统一单位生成与注册。
  - `Enum.cs` 中集中定义 Team / Order / AttackState / RPGState / Sweeplight 等枚举，便于扩展。

---

## 代码结构概览（选摘）

- 单位与战斗
  - `Assets/Unit.cs`：单位容器，负责聚合各组件。
  - `Assets/AttackComponent.cs`：攻击逻辑、攻击状态机（Chase/AttackCD）、扇形范围命中判定。
  - `Assets/MoveComponent.cs`：移动与追击逻辑。
  - `Assets/HealthComponent.cs`：生命值与伤害处理。
  - `Assets/AnimationComponent.cs`：Animator 封装（播放、长度、归一化时间）。

- 状态与模式
  - `Assets/UnitStateMachine.cs`：RPG 状态机与 `RPGInput` 定义。
  - `Assets/RPGUnitController.cs`：RPG 模式下的输入采集与状态驱动、攻击朝向与攻击范围显示。
  - `Assets/GameModeManager.cs`：RTS/RPG 模式管理与统一攻击入口。
  - `Assets/Enum.cs`：游戏内枚举（Team、Order、AttackState、RPGState、Sweeplight 等）。

- 表现与 Shader
  - `Assets/AttackRangeIndicator.cs`：攻击范围扫光组件（World UI + MaterialPropertyBlock）。
  - `Assets/Shader/AttackRangeIndicator.shader`：扇形/圆形/矩形范围 + 扫光特效。

- 导航与编队
  - `Assets/HexFormationPlanner.cs` / `Assets/IFormationPlanner.cs`：编队插槽生成与分配。
  - `Assets/NavigationAgentComponent.cs`：寻路代理。
  - `Assets/RVOManager.cs`：RVO 相关管理。

- 数据与管理
  - `Assets/UnitData.cs` / `Assets/UnitDataComponent.cs`：单位数据。
  - `Assets/UnitRegistry.cs`：单位注册与查找。
  - `Assets/SpawnManager.cs`：出生点与单位生成。
  - `Assets/Singleton.cs`：通用单例模板。

---

## 设计亮点（面试可重点讲解）

- **组件化 Unit 架构**  
  `Unit` 仅作为聚合点，实际逻辑拆分在多个小组件中，便于扩展和复用，同一个 Unit 架构可同时支撑玩家、敌人、RTS 小兵等不同角色。

- **模式抽象：RTS 与 RPG 共存**  
  通过 `GameMode` 抽象出 RTS/RPG 两种玩法模式，并在 `GameModeManager` 中集中管理。  
  这样“攻击/移动”这类操作有统一入口，不需要在各处手写 `if (isRPG) ... else ...`。

- **攻击范围：逻辑 + 表现一体化**  
  - C# 层用 OverlapSphere + 角度过滤做真正命中判定。  
  - Shader 层用扇形/圆形/矩形数学公式做像素级裁剪与扫光特效。  
  - 两者共用同一套半径/角度参数，保证“看见的”和“打到的”是一致的。

- **MaterialPropertyBlock 的使用**  
  攻击范围特效通过 `MaterialPropertyBlock` 传参，避免了修改共享材质导致所有单位一起变化的问题，体现了对 Unity 渲染与性能的基本理解。

---

## 后续规划（Roadmap 简要）

这个项目目前已经具备 Unit/战斗/状态机/攻击范围展示等核心玩法原型，后续可以围绕以下方向继续演进：

- **UI 系统**
  - 搭建 `UIRoot + UIManager + UIScreen` 的多界面管理框架。
  - 完成 HUD（血条、任务追踪、小地图等）与菜单/背包/技能树等 Screen。
  - 使用 World-Space UI 扩展头顶血条、任务标记与交互提示。

- **任务与对话系统**
  - 使用 ScriptableObject 定义任务数据与对话节点图。
  - 实现 `QuestSystem` 与 `DialogueController`，并通过 UI 展示与交互。

- **战斗扩展**
  - 抽象“技能/攻击配置”（伤害、形状、范围、前后摇等），实现完全数据驱动的技能系统。
  - 丰富 Reaction 系统（硬直、击飞、元素反应等）。

---

## 许可说明

本项目主要用于个人学习与面试展示，未特别指定开源协议。如需在商业项目中使用，请先与作者沟通确认。


