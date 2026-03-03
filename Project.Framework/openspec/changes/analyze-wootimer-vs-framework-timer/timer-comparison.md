# WooTimer vs Framework Timer 对比分析（源码实证）

## 1. 分析范围

- WooTimer 源码：`F:/Ugit/Frameworks/Unity-Framework/WooTimer-main`
- Framework 源码：`F:/Ugit/Frameworks/Unity-Framework/Project.Framework`
- 目标：比较两个 Timer 方案在能力、架构、风险与工程可维护性上的差异，输出决策结论。

## 2. WooTimer 解读

### 2.1 架构与调度链路

- 运行时入口：`TimerScheduler_Runtime` 在首次访问 `Instance` 时创建 `Timer` GameObject，`DontDestroyOnLoad`（`TimerScheduler_Runtime.cs:19-40`）。
- 帧驱动：`TimerScheduler_Runtime.Update -> TimerScheduler.Update`（`TimerScheduler_Runtime.cs:38-41`）。
- 调度器维护两类集合：
  - `List<TimerContext> timers`（普通计时上下文）
  - `List<ITimerGroup> groups`（组合上下文）
  - 见 `TimerScheduler.cs:16-27`。
- 编辑器模式下单独创建 `editorScheduler`，通过 `EditorApplication.update` 驱动（`TimeEx.cs:68-92`）。

### 2.2 API 与语义

- 统一入口在 `TimeEx` 静态扩展：
  - `Delay/Tick/DelayAndTick`（`TimeEx.cs:236-255`）
  - `While/Until/DoWhile/DoUntil`（`TimeEx.cs:211-235`）
  - `Sequence/Parallel`（`TimeEx.cs:195-208`）
- fluent 控制链：
  - `OnBegin/OnTick/OnComplete/OnCancel`（`TimeEx.cs:126-145`）
  - `Pause/UnPause/Stop/Cancel/SetTimeScale/SetId`（`TimeEx.cs:147-177`）
  - `AddTo(owner)` 与 `KillTimers(owner)`（`TimeEx.cs:106-121`）。
- `await` 支持：通过自定义 awaiter 监听 `OnComplete`（`TimeEx.cs:19-57`）。

### 2.3 数据结构与生命周期

- 核心上下文基类 `TimerContextBase` 包含：
  - `id`、`owner`、`timeScale`、`onBegin/onTick/onComplete/onCancel`
  - `Stop` 与 `Cancel` 均走 `_Cancel`，区别在于是否触发 `onCancel`
  - 完成时 `Complete()` 回调并 `TimeEx.Cycle(this)` 回收（`TimerContextBase.cs:87-116`）。
- `TimerContext.Update` 逻辑：`delta *= timeScale` 后推进 time（`TimerContext.cs:25-29`）。
- 对象池：`SimpleObjectPool<T>`，上下文回收后复用（`SimpleObjectPool.cs:55-99` 与 `TimerScheduler.cs:44-70`）。

### 2.4 关键特点

- 优点：
  - 能力覆盖广（条件轮询、组合执行、上下文生命周期绑定、await）。
  - API 使用体验好，业务表达力强。
  - 有编辑器观察工具 `TimerWatcher`（`Assets/WooTimer/Editor/TimerWatcher.cs`）。
- 限制：
  - 时间源固定 `Time.deltaTime`（无原生 unscaled 分流，`TimeEx.cs:60-67`）。
  - 回调未做统一异常保护（`Invoke*` 直接调用）。
  - 线程模型是主线程调度，无并发隔离。

## 3. Framework Timer 解读

### 3.1 架构与调度链路

- `RootModule.Update` 每帧调用 `ModuleSystem.Update(GameTime.deltaTime, GameTime.unscaledDeltaTime)`（`RootModule.cs:140-144`）。
- `ModuleSystem.Update` 轮询所有 `IUpdateModule`（`ModuleSystem.cs:29-41`）。
- `TimerModule` 作为 `IUpdateModule` 在 `Update()` 中分别更新 scaled/unscaled 桶（`TimerModule.cs:155-159`）。

### 3.2 API 与语义

- 接口 `ITimerModule` 提供：
  - `AddTimer`（两个重载）
  - `Pause/Resume/IsRunning/GetLeftTime/Restart`
  - `ResetTimer`（两个重载）
  - `RemoveTimer/RemoveAllTimer`
  - 见 `ITimerModule.cs:3-35`。
- 典型业务使用：UI 隐藏后延迟关闭（`UIModule.cs:409-412`）。

### 3.3 数据结构与生命周期

- 核心容器：
  - `_timers: Dictionary<int, Timer>`
  - `_timerBuckets: Dictionary<bool, List<Timer>>`
  - `false` 为 scaled，`true` 为 unscaled（`TimerModule.cs:49-54`）。
- 对象池存在：`TimerPool<Timer>`（`TimerModule.cs:55-62`）。

### 3.4 关键风险

- 高风险问题：一次性计时器触发后仅执行 `_timers.Remove` 与 `_pool.Release`，未从 bucket 列表物理移除（`TimerModule.cs:180-183`）。
  - 代码中 `list.RemoveAt(i)` 处于注释状态（`TimerModule.cs:181`）。
  - 影响：
    - 列表保留陈旧引用，遍历成本随时间增加。
    - 对象复用后可能在 bucket 中形成重复引用，导致重复更新。
    - 若旧引用与新配置跨 scaled/unscaled 桶，可能出现同一对象被两次更新的异常行为。
- 中风险问题：`RemoveAllTimer` 清空容器但未把对象逐个归还池（`TimerModule.cs:148-153`），复用效率下降。
- 中风险问题：回调异常没有隔离保护，可能影响模块更新流程。

## 4. 对比结论

| 维度 | WooTimer | Framework Timer |
| --- | --- | --- |
| 调度模型 | 独立 `TimerScheduler` + Context 抽象 | Framework `IUpdateModule` 集成 |
| 时间源 | `Time.deltaTime`（编辑器有自定义 delta） | scaled/unscaled 双时间源 |
| 任务表达力 | Delay/Tick/While/Until + Sequence/Parallel + await | 基础延时/循环/重置 |
| 生命周期绑定 | `AddTo(owner)` + `KillTimers(owner)` | 以 `timerId` 手动管理 |
| 对象池策略 | 上下文回收后从调度集合移除并回池 | 存在 bucket 残留风险 |
| 工具化 | 内置 `TimerWatcher` | 无原生可视化 |
| 异常策略 | 无统一捕获 | 无统一捕获 |

## 5. 决策建议

### 5.1 可直接借鉴

- 引入“上下文化 API”思路（链式 `OnComplete/OnCancel/SetTimeScale`）。
- 引入 `AddTo(owner)` 生命周期绑定，降低 `timerId` 泄漏风险。
- 引入可视化调试面板（参考 `TimerWatcher` 的分配/回收跟踪事件）。

### 5.2 需改造后再借鉴

- `Sequence/Parallel` 组合能力可作为中长期增强项，但要先与当前 ModuleSystem 生命周期统一。
- `await` 化接口可加在高层包装，而非直接替换底层 `ITimerModule`。

### 5.3 暂不建议直接引入

- 不建议直接整库替换当前 `TimerModule`：
  - 当前框架已深度绑定 ModuleSystem 与现有业务入口。
  - WooTimer 默认时间源不区分 unscaled，语义与现有接口不完全对齐。

## 6. 总结

WooTimer 的优势在“表达力与可用性”，当前框架 Timer 的优势在“轻量集成”。但当前框架实现中存在明确的高风险一致性问题（一次性任务 bucket 残留），优先级应高于功能增强。建议策略是“先修正确性，再增强调度能力”，而不是直接替换底层定时器实现。
