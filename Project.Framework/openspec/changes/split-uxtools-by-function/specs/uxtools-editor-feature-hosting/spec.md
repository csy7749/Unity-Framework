## ADDED Requirements

### Requirement: UnityFramework SHALL host UXTools editor entrypoints
系统 MUST 在 `UnityFramework` 菜单域提供 UXTools Editor 能力入口，以统一工具访问路径。

#### Scenario: Component library entry is available
- **WHEN** 用户在 Unity Editor 打开菜单 `UnityFramework/UXTools`
- **THEN** 系统 MUST 提供“组件库”入口并可打开对应窗口

#### Scenario: Recent opened prefabs entry is available
- **WHEN** 用户在 Unity Editor 打开菜单 `UnityFramework/UXTools`
- **THEN** 系统 MUST 提供“最近打开模板”入口并可打开对应窗口

#### Scenario: Recent selected files entry is available
- **WHEN** 用户在 Unity Editor 打开菜单 `UnityFramework/UXTools`
- **THEN** 系统 MUST 提供“最近选中文件”入口并可打开对应窗口

### Requirement: Editor-only UXTools code SHALL remain outside runtime assemblies
系统 MUST 保证仅编辑器功能不进入运行时程序集编译路径。

#### Scenario: Editor assembly boundary is enforced
- **WHEN** 项目进行非 Editor 目标编译
- **THEN** 仅 Editor 功能代码 MUST NOT 被运行时程序集直接引用

#### Scenario: Editor namespace usage is isolated
- **WHEN** 运行时程序集存在 `UnityEditor` API 调用需求
- **THEN** 相关代码 MUST 通过 `#if UNITY_EDITOR` 或 Editor 文件隔离

### Requirement: Editor feature migration SHALL be function-based and traceable
系统 MUST 按功能包进行拆分迁移，并保留可追踪迁移状态。

#### Scenario: Migration unit is defined by feature
- **WHEN** 执行 UXTools Editor 迁移
- **THEN** 迁移批次 MUST 以“组件库/最近打开/最近选中/检查器工具”等功能包为单位

#### Scenario: Migration progress is verifiable
- **WHEN** 任一功能包完成迁移
- **THEN** 变更记录 MUST 包含受影响文件范围、程序集边界和验收结果
