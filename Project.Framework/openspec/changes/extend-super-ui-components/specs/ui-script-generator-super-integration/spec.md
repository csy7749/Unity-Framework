## ADDED Requirements

### Requirement: Script generator SHALL recognize and emit bindings for Super components
系统 MUST 在现有 `UIScriptGenerator` 链路中识别 Super 组件，并生成可编译的 `_Auto` 绑定代码。

#### Scenario: Super component selected in provider
- **WHEN** 开发者在节点提供器中添加 `Super*` 组件字段
- **THEN** 生成器 MUST 在 `_UGUINodeProvider` 与 `_Auto` 中输出对应强类型字段

#### Scenario: Generated code compiles
- **WHEN** 执行 UI 自动生成流程
- **THEN** 输出的 `_Auto/_View/_UGUINodeProvider` MUST 在当前工程编译通过

### Requirement: Button/Toggle/Slider event generation SHALL support subclass types
系统 MUST 基于可赋值关系（而非仅字符串精确匹配）生成按钮/开关/滑条事件绑定，以兼容 `SuperButton` 等继承组件。

#### Scenario: SuperButton event binding
- **WHEN** 控件类型为 `SuperButton`
- **THEN** 生成器 MUST 生成 `onClick.AddListener(...)` 与对应移除逻辑

#### Scenario: Existing component compatibility
- **WHEN** 控件类型为原生 `Button/Toggle/Slider`
- **THEN** 生成结果 MUST 与变更前行为一致

### Requirement: Script generator SHALL provide specialized stubs for Super list/select components
系统 MUST 为 `SuperToggle`、`SuperPageScrollView`、`UILoopScroll`、`UILoopScrollMul`、`UILoopScrollAndToggle` 生成初始化与回调桩代码。

#### Scenario: SuperToggle stub generation
- **WHEN** 生成器处理 `SuperToggle`
- **THEN** `_Auto` MUST 生成 `SetOnToggleItemClick` 绑定及回调方法桩

#### Scenario: LoopScroll stub generation
- **WHEN** 生成器处理 `UILoopScroll*`
- **THEN** `_Auto` MUST 生成列表初始化与数据回调方法桩

### Requirement: Script generator SHALL support SuperText localization init hook
系统 MUST 在生成流程中为 `SuperText` 提供初始化期语言设置调用入口。

#### Scenario: SuperText auto language init
- **WHEN** 生成器处理 `SuperText` 字段
- **THEN** `_Auto` MUST 生成可用于语言初始化的调用代码（例如 `SetLanguage()`）
