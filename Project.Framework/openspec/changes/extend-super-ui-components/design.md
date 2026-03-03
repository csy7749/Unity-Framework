## Context

当前项目的 UI 组件扫描与自动生成链路位于 `Assets/Editor/UIScriptGenerator`，其事件生成仅对 `Button/Toggle/Slider` 做了固定分支处理。
参考项目中 Super 系列组件与生成链路已经形成闭环，但源码依赖了当前项目不存在的类型（例如 `BaseUIWidget/BaseUIView/BaseUICtrl` 以及第三方 LoopScrollRect 插件实现）。
因此本次设计目标是：在不引入外部强依赖的前提下，为当前框架提供可编译、可挂载、可生成代码的 Super 组件能力。

## Goals / Non-Goals

**Goals:**
- 在当前项目新增 `Super*` 与 `UILoopScroll*` 组件，满足常用 UI 扩展能力。
- 扩展 `UIScriptGenerator`，使其支持 Super 组件的自动绑定与事件模板生成。
- 保持现有 `Button/Toggle/Slider` 生成行为不回归。
- 所有新增代码保持在当前工程可编译边界内，不引入外部插件硬依赖。

**Non-Goals:**
- 不完整复刻参考项目中所有业务侧特性（例如完整对象池系统、业务 UI 基类耦合逻辑）。
- 不改造现有 UI 业务架构（Window/Widget 体系）。
- 不新增外部 package 依赖。

## Decisions

### 决策 1：组件命名与命名空间兼容现有生成模板
- 新增组件统一放在 `UnityEngine.UI` 命名空间下，保证生成代码已有 `using UnityEngine.UI` 即可直接引用类型。
- 理由：最小化模板改动，减少生成代码编译风险。
- 备选方案：新建独立命名空间并改模板自动注入 `using`；被放弃，因为会增加模板复杂度与兼容成本。

### 决策 2：采用“等价适配实现”而不是直接拷贝参考实现
- `SuperButton/SuperText/SuperToggle/...` 提供文档要求的核心公开 API 与可用行为。
- 对依赖外部系统的能力（如复杂对象池、外部资产系统）采用本框架内替代实现或 no-op 安全实现。
- 理由：确保当前项目可独立编译运行。
- 备选方案：原样拷贝参考项目源码；被放弃，因为会引入大量缺失类型与三方依赖。

### 决策 3：生成器按“类型可赋值”而非“字符串完全匹配”处理通用事件
- 在 `UICopyEditor` 中将 `Button/Toggle/Slider` 绑定逻辑扩展为：若控件类型可赋值到对应基类，也生成同类事件绑定。
- 理由：自动兼容 `SuperButton` 等继承组件，且不会破坏旧逻辑。

### 决策 4：为 Super 专项组件增加特化模板生成分支
- 为 `SuperToggle`、`SuperPageScrollView`、`UILoopScroll`、`UILoopScrollMul`、`UILoopScrollAndToggle` 增加专用代码桩输出。
- 为 `SuperText` 增加 `SetLanguage()` 初始化语句（满足 ignoreLocalization 场景可配置）。
- 理由：对齐参考项目文档中的“组件 -> 自动生成代码”映射能力。

### 决策 5：LoopScroll 采用框架内轻量实现
- 提供 `UILoopScroll*` 组件与统一回调接口，行为基于 `ScrollRect` 的最小可用实现。
- 理由：避免硬依赖第三方 LoopScrollRect 插件，仍满足生成器和业务侧接入。

## Risks / Trade-offs

- [风险] 与参考项目行为存在细节差异（尤其是对象池与复杂拖拽逻辑）
  - Mitigation：优先保证公开 API 与生成链路一致，复杂行为后续增量补齐。
- [风险] 生成器分支增多可能引入模板回归
  - Mitigation：保留原有 `Button/Toggle/Slider` 分支路径并增加类型回归检查。
- [风险] LoopScroll 轻量实现在大数据量场景性能不及第三方插件
  - Mitigation：先提供兼容 API；后续如需高性能再切换到插件实现。

## Migration Plan

1. 新增 Super 运行时组件与 LoopScroll 组件文件，确保运行时编译通过。
2. 修改 `UIScriptGenerator` 的事件绑定与特化模板输出逻辑。
3. 在示例/现有 UI prefab 上执行一次生成流程，确认 `_Auto/_View/_UGUINodeProvider` 可生成并编译。
4. 完成基础冒烟验证（按钮点击、文本语言调用、Toggle 回调、列表初始化）。
5. 若出现兼容问题，回滚生成器分支改动并保留组件类文件作为独立能力。

## Open Questions

- `SuperText` 的语言切换是否需要直接接入 `UnityFramework.LocalizationModule`，还是继续沿用 `UXText` 现有机制。
- 是否需要在本变更内补充 `Super*` 的自定义 Inspector（参考项目包含 Editor 扩展）。
- LoopScroll 后续是否计划切换到第三方虚拟化滚动实现。
