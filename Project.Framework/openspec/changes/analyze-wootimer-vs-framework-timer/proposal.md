## Why

当前项目存在多个计时实现（`UnityFramework.TimerModule`、HotFix 示例 `TimerManager`、UXTools `GameTimer`），缺少统一的横向基准与能力边界说明。团队已经提供本地 `WooTimer` 源码（`F:/Ugit/Frameworks/Unity-Framework/WooTimer-main`），需要一次基于源码证据的系统化对比结论。

## What Changes

- 新增一套基于 OpenSpec 的“Timer 对比分析”文档产物，用于沉淀可追溯的技术结论。
- 对 `WooTimer` 本地源码进行能力与实现模式解读，并给出关键代码证据。
- 对本项目 `UnityFramework` 计时模块进行源码级分析，覆盖调度链路、API 语义、生命周期与风险。
- 输出差异矩阵与结论，明确“可直接借鉴 / 需改造 / 暂不建议引入”的条目。
- **非代码改动**：本变更不修改运行时代码，只交付分析文档与后续实施任务清单。

## Capabilities

### New Capabilities
- `timer-comparison-analysis`: 形成可追溯的 Timer 框架对比文档，覆盖能力、架构、风险与建议。

### Modified Capabilities
- 无

## Impact

- 影响范围：`openspec/changes/analyze-wootimer-vs-framework-timer/` 文档产物。
- 涉及源码阅读范围（只读分析）：`Assets/UnityFramework/Runtime/Module/TimerModule/*`、`Assets/UnityFramework/Runtime/Core/ModuleSystem.cs`、`Assets/UnityFramework/Runtime/Module/RootModule.cs`、`Assets/GameScripts/HotFix/GameLogic/GameModule.cs`、`Assets/GameScripts/HotFix/GameLogic/Module/UIModule/*`。
- 增加外部仓库本地镜像读取范围：`F:/Ugit/Frameworks/Unity-Framework/WooTimer-main/Assets/WooTimer/Runtime/*` 与 `README.md`。
