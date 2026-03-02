## Why

当前 UXTools 编辑器“设置-通用-语言”仍保留多语言切换能力，且界面文案依赖 `EditorLocalization.GetLocalization`。当前需求仅保留中文，因此需要移除该入口的多语言能力并改为中文直写。

## What Changes

- 调整 `ConfigurationWindow`（设置-通用）中的语言项为固定“简体中文”，不可切换。
- 移除该窗口内对 `EditorLocalization.GetLocalization` 的依赖，改为直接中文文案。
- 调整 `EditorLocalizationSettings` 默认值与写入逻辑，统一为中文。
- 不改动运行时国际化模块和 UXGUI 文本表链路。

## Capabilities

### New Capabilities
- `uxtools-editor-chinese-only-localization`: UXTools 编辑器设置窗口语言配置固定为中文，并使用中文直写文案。

### Modified Capabilities
- None

## Impact

- 受影响目录：
  - `Assets/UXTools/Editor/Tools/UXTools/Window_Custom/Configuration/*`
  - `Assets/UXTools/Editor/Common/EditorLocalization/Settings/*`
- 受影响行为：
  - 设置窗口“通用-语言”交互
  - 设置窗口相关文案来源
  - 编辑器语言设置默认与保存结果
