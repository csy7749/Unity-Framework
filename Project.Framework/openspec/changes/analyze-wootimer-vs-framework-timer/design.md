## Context

- 目标：形成一份可复用、可追溯的 Timer 框架对比分析。
- WooTimer 侧证据：本地仓库 `F:/Ugit/Frameworks/Unity-Framework/WooTimer-main`，重点文件包括：
  - `Assets/WooTimer/Runtime/TimeEx.cs`
  - `Assets/WooTimer/Runtime/Scheduler/TimerScheduler.cs`
  - `Assets/WooTimer/Runtime/Scheduler/TimerScheduler_Runtime.cs`
  - `Assets/WooTimer/Runtime/Context/*`
  - `README.md`
- 当前框架侧证据：本仓库 `Project.Framework` 中 Timer 相关实现：
  - `Assets/UnityFramework/Runtime/Module/TimerModule/*`
  - `Assets/UnityFramework/Runtime/Core/ModuleSystem.cs`
  - `Assets/UnityFramework/Runtime/Module/RootModule.cs`
  - `Assets/GameScripts/HotFix/GameLogic/GameModule.cs`
  - `Assets/GameScripts/HotFix/GameLogic/Module/UIModule/*`

## Goals / Non-Goals

**Goals:**
- 形成一份基于源码证据的对比文档，覆盖能力、架构、内存行为、异常语义与可维护性。
- 给出对本项目 Timer 体系的可落地方向（文档级决策，不做代码改造）。

**Non-Goals:**
- 不在本次变更中重构或替换 `TimerModule`。
- 不引入第三方依赖。
- 不对 HotFix 中独立计时器实现做统一改造。

## Decisions

### 1) 使用 `timer-comparison-analysis` 能力承载输出
- 备选方案：直接写普通文档，不走 OpenSpec。
- 决策原因：OpenSpec 产物可直接转入后续实施阶段，减少二次整理成本。

### 2) 对比维度固定为 8 类
- 维度：调度模型、时间源与缩放、API 语义、生命周期、内存行为、并发安全、错误处理、测试与可验证性。
- 备选方案：只做 API 列表对比。
- 决策原因：仅列功能无法支撑框架选型，必须包含运行时行为和工程约束。

### 3) 文档中强制记录“关键行为证据”
- WooTimer 关键行为：`TimeEx` fluent API、`TimerContext` 抽象、`Sequence/Parallel` 组合、对象池回收、`AddTo(owner)` 生命周期绑定。
- 框架关键行为：`RootModule -> ModuleSystem -> TimerModule` 调度链，双桶存储，ID 管理，以及一次性任务回收逻辑。
- 决策原因：确保每个结论可回溯到源码行为，而不是抽象印象。

## Risks / Trade-offs

- [风险] 仅交付文档，短期不会修复已识别风险 -> 缓解：任务中保留可直接实施的后续项。
- [风险] 多套计时实现并存导致语义分叉 -> 缓解：明确每套 Timer 的适用场景和边界。
- [风险] 对比结论可能引发“全量替换”误解 -> 缓解：文档显式给出“借鉴优先、替换慎用”的决策建议。

## 比较分析正文

### WooTimer（本地源码实证）核心特征

- API 设计为 fluent + extension 风格：`Delay/Tick/DelayAndTick/While/Until/DoWhile/DoUntil`，并支持 `await`。
- 组合能力原生支持：`Sequence()` 与 `Parallel()` 返回 `ITimerGroup`，内部可拼接多个 `ITimerContext`。
- 生命周期绑定：`AddTo(owner)` + `owner.KillTimers()`。
- 调度主循环：`TimerScheduler_Runtime.Update -> TimerScheduler.Update -> TimerContext.Update`。
- 时间基准：运行时使用 `Time.deltaTime`；编辑器模式使用 `EditorApplication.update` 与自算 `delta_editor`。
- 任务控制：`Pause/UnPause/Stop/Cancel/SetTimeScale/SetId`。

### 本项目 TimerModule（源码实证）核心特征

- 调度链路：`RootModule.Update` 调用 `ModuleSystem.Update`，后者轮询 `IUpdateModule`，其中 `TimerModule` 执行 `UpdateTimers(false/true)`。
- 存储结构：`_timers`（ID->Timer）+ `_timerBuckets[bool]`（scaled/unscaled）。
- API：`AddTimer/ResetTimer/Pause/Resume/Restart/RemoveTimer/RemoveAllTimer`，参数模型为秒值 + `params object[]`。
- 错误策略：回调异常不在模块内捕获。
- 复用策略：有 `TimerPool<Timer>`，但一次性任务触发后未从 bucket 物理移除，存在数据结构一致性风险。

### 差异矩阵（结论版）

| 维度 | WooTimer（源码） | 当前 TimerModule（源码） | 结论 |
| --- | --- | --- | --- |
| 时间粒度 | 秒级 API + 近似帧 API(`Frame()`) | 秒级 API，scaled/unscaled 双时间源 | 若需要条件轮询/组合定时，WooTimer 能力更完整 |
| 任务模型 | `ITimerContext` 抽象，可组合 | 轻量 `Timer` 结构，单层任务模型 | 框架侧简单直接，WooTimer 偏 DSL 化 |
| 生命周期 | `AddTo(owner)` + `KillTimers(owner)` | 手动持有 `timerId` 并 Remove | WooTimer 更适合与对象生命周期绑定 |
| 时间缩放 | 上下文 `SetTimeScale` | 仅 `Unscaled` 维度切换 | 若需要局部变速，当前框架能力不足 |
| 内存行为 | 回收时从调度列表移除并回池 | 一次性触发后未从 bucket 移除 | 框架侧存在高风险一致性问题 |
| 错误处理 | 回调未做 try/catch | 回调未做 try/catch | 两侧均需策略化异常边界 |
| 可观测性 | 提供 `TimerWatcher` 编辑器窗口 | 无原生可视化 | WooTimer 的诊断能力可借鉴 |
