## Context

参考框架 `RedDotNew` 已验证一套完整红点链路：配置资产驱动路径、数量型与查看型节点并存、支持本地持久化和周期性重置。当前项目 `GameLogic` 仍以旧版布尔红点为主，无法覆盖上述能力。  
本次变更要求是在 `Assets/GameScripts/HotFix/GameLogic` 内新增一套独立实现，并保持旧系统不受影响。

## Goals / Non-Goals

**Goals:**
- 在 `GameLogic/Common/RedDotNew/Runtime` 落地运行时红点核心类型。
- 支持 `Register/Unregister` 并在注册时立即回调当前值。
- 支持数量型 API：`ChangeRedDotCount`、`ChangeRedDotCountByAccumulation`。
- 支持查看型 API：`SetWaitWatch`、`Watch`，并包含 `Once/Day/Week/Month` 规则。
- 支持 `PlayerPrefs` 本地存储和 `bindRole` 键前缀能力。
- 提供可复用 `RedDotViewBase`，支持 `Normal/New/Number` 三种显示策略。

**Non-Goals:**
- 不替换旧版 `Common/RedNote` 与 `UXTools` 红点系统。
- 不在本次变更内接入具体业务模块或 UI 预制体。
- 不实现配置编辑器和枚举自动生成功能。

## Decisions

### 决策 1：新增命名空间隔离实现，不改旧系统
- 方案：新增 `GameLogic.RedDotNew` 命名空间和独立目录。
- 原因：避免与现有 `Reddot`/`RedNote` 类型冲突，降低回归风险。
- 备选：原地升级旧 `ReddotManager`。  
  放弃原因：影响面过大，难以保证现有 UI 行为不变。

### 决策 2：以配置资产作为红点 ID 与路径的唯一映射源
- 方案：新增 `RedDotConfigAsset`，在运行时构建 `DataDic`，树初始化时按配置预建非参数化节点。
- 原因：与参考框架一致，支持路径参数化并便于扩展。
- 备选：纯代码硬编码路径。  
  放弃原因：维护成本高，无法统一管理展示类型与持久化策略。

### 决策 3：节点按“中间节点数量型 + 叶子按配置类型”构建
- 方案：`RedDotNodeBase` 负责路径树，非叶子统一 `RedDotNumberNode`，叶子根据 `IsView` 创建 `RedDotViewNode` 或 `RedDotNumberNode`。
- 原因：中间层统一聚合逻辑，避免多种父节点计算分叉。
- 备选：中间节点继承配置类型。  
  放弃原因：聚合计算复杂且不符合红点树常见模型。

### 决策 4：本地存储仅做显式透明持久化，不加静默兜底
- 方案：读取失败或路径缺失时按当前状态直接返回，不引入默认“伪成功”逻辑。
- 原因：遵循调试优先，错误可见。
- 备选：异常时强制吞错和默认值回退。  
  放弃原因：隐藏问题来源，增加排障成本。

## Risks / Trade-offs

- [Risk] 新旧两套红点系统并存，业务可能误用旧 API。  
  Mitigation：新系统放入独立目录与命名空间，并通过 OpenSpec 明确目标 API。

- [Risk] `UniqueKey` 未初始化时，`bindRole` 会退化为裸 key。  
  Mitigation：保留显式 `UniqueKey` 字段，调用侧可在登录后赋值；不做隐式猜测。

- [Risk] 未接入 Unity 实际场景时，UI 行为验证不完整。  
  Mitigation：本变更先保证运行时 API 闭环和静态可编译，UI 接入在业务使用时验证。

## Migration Plan

1. 新增 OpenSpec artifacts，定义能力与验收场景。
2. 在 `GameLogic/Common/RedDotNew/Runtime` 创建核心运行时代码。
3. 按任务清单完成实现并更新勾选状态。
4. 扫描关键 API 与命名冲突，确保无直接覆盖旧系统类型。

## Open Questions

- `RedDotConfigAsset` 在当前项目的具体资源加载路径和出包策略是否需要统一约定（本次先保持 `Resources.Load` 默认约定）。
