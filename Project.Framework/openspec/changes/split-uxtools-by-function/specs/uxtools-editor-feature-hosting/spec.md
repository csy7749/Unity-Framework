## ADDED Requirements

### Requirement: UnityFramework SHALL host UXTools editor entrypoints
系统 MUST 在 `UnityFramework` 菜单域提供 UXTools Editor 入口，统一访问路径。

#### Scenario: Component library entry is available
- **WHEN** 用户打开 `UnityFramework/UXTools`
- **THEN** 系统 MUST 提供组件库入口并可打开窗口

#### Scenario: Recent opened prefabs entry is available
- **WHEN** 用户打开 `UnityFramework/UXTools`
- **THEN** 系统 MUST 提供最近打开模板入口并可打开窗口

#### Scenario: Recent selected files entry is available
- **WHEN** 用户打开 `UnityFramework/UXTools`
- **THEN** 系统 MUST 提供最近选中文件入口并可打开窗口

### Requirement: Editor migration SHALL be function-complete per batch
系统 MUST 以“功能闭包”作为迁移批次单位，避免只迁移入口不迁移依赖。

#### Scenario: Batch includes dependency closure
- **WHEN** 迁移任一 Editor 功能
- **THEN** 该批次 MUST 包含窗口、设置、数据、工具和资源常量等直接依赖

#### Scenario: Batch validation is recorded
- **WHEN** 迁移批次完成
- **THEN** 变更记录 MUST 包含编译结果与功能回归结果

### Requirement: Editor-only code SHALL remain isolated from runtime assemblies
系统 MUST 保证 Editor-only 代码不泄漏到运行时程序集。

#### Scenario: Player target compile does not include editor-only code
- **WHEN** 执行非 Editor 目标编译
- **THEN** 运行时程序集 MUST NOT 直接依赖 Editor-only 代码

#### Scenario: Editor APIs are isolated
- **WHEN** 代码使用 `UnityEditor` API
- **THEN** 相关代码 MUST 位于 Editor 程序集或由 `UNITY_EDITOR` 条件隔离
