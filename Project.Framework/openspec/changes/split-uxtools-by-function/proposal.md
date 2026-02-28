## Why

`Assets/UXTools` 目前同时承载 Editor 工具与运行时 UGUI 组件，职责边界不清，导致热更程序集接入、编译边界（Editor vs Runtime）和后续扩展都存在高维护成本。现在需要按功能完成结构化拆分，先明确规范，再实施迁移。

## What Changes

- 将 UXTools 的 Editor-only 功能统一纳入 `UnityFramework.Editor` 能力域与入口管理。
- 将与 UGUI 组件相关且未来可能热更扩展的运行时代码纳入 `GameLogic` 能力域。
- 建立可执行的功能拆分规则：按功能分类、按程序集边界落位、按依赖方向约束。
- 定义迁移验收标准：编译通过、菜单入口可用、热更侧绑定链路可工作、无跨边界非法依赖。

## Capabilities

### New Capabilities
- `uxtools-editor-feature-hosting`: 规范并落地 UXTools Editor 功能（如组件库、最近打开、最近选中）在 UnityFramework 的托管方式与入口。
- `uxtools-hotfix-ugui-runtime`: 规范并落地 UGUI 运行时功能在 GameLogic 的接入方式，支持后续热更扩展。

### Modified Capabilities
- None

## Impact

- 影响目录：
  - `Assets/UXTools/*`
  - `Assets/UnityFramework/Editor/*`
  - `Assets/GameScripts/HotFix/GameLogic/*`
  - `Assets/Editor/UIScriptGenerator/*`
- 影响程序集：
  - `UnityFramework.Editor`
  - `GameLogic`
  - `Leihuo.UXTools.Runtime` / `Leihuo.UXTools.Editor*`
- 影响点：
  - 菜单入口、组件映射、MVVM 绑定代理、编译边界（`UNITY_EDITOR` / Runtime）。
