## Why

当前 `Assets/UXTools` 同时包含 Editor 工具与运行时代码，职责边界不清晰，导致维护和迁移成本高。  
你已回退到上一版本，当前版本不包含 `SuperButton/SuperToggle` 等 super 系列扩展，本变更需要按该事实更新拆分方案。

## What Changes

- 按功能拆分 UXTools 代码，而不是仅修改程序集引用。
- 优先迁移 Editor 功能到 `UnityFramework.Editor`（组件库、最近打开、最近选中及其依赖）。
- 运行时相关代码后续按批迁移到 `GameLogic`，但仅基于当前版本已有能力。
- 明确约束：本变更不引入 super 系列扩展，不新增 `Super*` 组件接入需求。

## Capabilities

### New Capabilities
- `uxtools-editor-feature-hosting`: 将 UXTools Editor 功能按模块迁入并由 UnityFramework 统一托管。
- `uxtools-hotfix-ugui-runtime`: 在不引入 super 系列扩展前提下，规划 UGUI 运行时功能的热更侧接入与迁移。

### Modified Capabilities
- None

## Impact

- 受影响目录：
  - `Assets/UXTools/*`
  - `Assets/UnityFramework/Editor/*`
  - `Assets/GameScripts/HotFix/GameLogic/*`
  - `Assets/Editor/UIScriptGenerator/*`
- 受影响程序集：
  - `UnityFramework.Editor`
  - `GameLogic`
  - `Leihuo.UXTools.Runtime`
  - `Leihuo.UXTools.Editor*`
- 受影响领域：
  - Editor 菜单入口与窗口托管
  - 代码迁移后的程序集边界
  - 热更侧 UGUI 接入（基于现有版本能力）
