## ADDED Requirements

### Requirement: Super runtime components SHALL be available in current framework
系统 MUST 提供可挂载、可编译、可在运行时调用的 Super 系列组件：`SuperButton`、`SuperText`、`SuperToggle`、`SuperToggleItem`、`SuperInputField`、`SuperDropdown`、`SuperPageScrollView`、`SuperImage`。

#### Scenario: Add component in inspector
- **WHEN** 开发者在 UI 对象上添加上述 Super 组件
- **THEN** 组件 MUST 可正常挂载且项目编译通过

#### Scenario: Core API invocation
- **WHEN** 业务代码调用组件公开 API（如点击、文本设置、选中状态切换）
- **THEN** 组件 MUST 执行对应逻辑且不抛出运行时异常

### Requirement: Loop scroll components SHALL be provided without hard third-party dependency
系统 MUST 提供 `UILoopScroll`、`UILoopScrollMul`、`UILoopScrollAndToggle` 组件，并且在未引入外部 LoopScrollRect 插件时仍可编译运行（允许轻量实现）。

#### Scenario: Project without third-party plugin
- **WHEN** 工程未安装第三方 LoopScrollRect 插件
- **THEN** `UILoopScroll*` 组件 MUST 仍可编译且基础 API 可调用

#### Scenario: Basic list initialization
- **WHEN** 业务调用列表初始化与刷新接口
- **THEN** 组件 MUST 能完成最小可用的数据驱动刷新流程

### Requirement: Super component public APIs SHALL align with migration baseline
系统 MUST 提供与迁移基线一致的关键公开接口（至少包含文档中高频接口），以支持现有业务迁移。

#### Scenario: SuperButton baseline APIs
- **WHEN** 调用 `SetSprite/SetText/SetLocationText/SetGrey/SetOutLine`
- **THEN** `SuperButton` MUST 提供对应方法并保持可调用

#### Scenario: SuperToggle baseline APIs
- **WHEN** 调用 `SetOnToggleItemClick/SetSelectedIndex/SetToggleItemCount`
- **THEN** `SuperToggle` MUST 提供对应方法并可触发回调

#### Scenario: SuperText baseline APIs
- **WHEN** 调用 `SetLanguage` 或触发语言刷新
- **THEN** `SuperText` MUST 提供语言相关入口并保持兼容行为
