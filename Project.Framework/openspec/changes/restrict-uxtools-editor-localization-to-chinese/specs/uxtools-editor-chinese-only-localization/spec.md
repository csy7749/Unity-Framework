## ADDED Requirements

### Requirement: Settings general language selector MUST be Chinese-only
系统在 `设置-通用-语言` 中 MUST 仅允许简体中文，不得提供可切换多语言选项。

#### Scenario: Language selector is fixed to Simplified Chinese
- **WHEN** 用户打开设置窗口的通用页
- **THEN** 语言控件 MUST 仅显示“简体中文”并且不可切换

#### Scenario: Save action always writes Chinese editor language
- **WHEN** 用户点击设置窗口确认
- **THEN** 系统 MUST 将编辑器语言设置写为 `EditorLocalName.Chinese`

### Requirement: Configuration window text MUST not depend on GetLocalization
系统在 `ConfigurationWindow` 中 MUST 不再调用 `EditorLocalization.GetLocalization`，而是直接使用中文文案。

#### Scenario: Window title and buttons are direct Chinese text
- **WHEN** 设置窗口初始化
- **THEN** 标题与确认/取消按钮 MUST 使用中文直写文案

#### Scenario: General and switch labels are direct Chinese text
- **WHEN** 用户浏览通用和功能开关区域
- **THEN** 相关标签 MUST 使用中文直写文案而不是本地化查询

### Requirement: Editor localization settings MUST default to Chinese
系统在创建或更新 `EditorLocalizationSettings` 时 MUST 使用中文作为目标语言。

#### Scenario: New settings default to Chinese
- **WHEN** 创建新的 `EditorLocalizationSettings`
- **THEN** `LocalType` MUST 默认为 `EditorLocalName.Chinese`

#### Scenario: ChangeLocalValue enforces Chinese
- **WHEN** 任意调用 `ChangeLocalValue`
- **THEN** 最终保存的 `LocalType` MUST 为 `EditorLocalName.Chinese`
