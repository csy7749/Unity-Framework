## Why

当前框架缺少 Super 系列 UI 组件与自动生成链路的配套能力，导致复杂交互（增强按钮、增强文本、Toggle 组、分页/循环列表）只能靠重复手写代码，UI 产出效率和一致性不足。
参考项目已经验证了该能力闭环（可识别、可生成、可绑定、可运行），需要在本框架内补齐等价扩展并对接现有 `UIScriptGenerator`。

## What Changes

- 新增运行时 Super 组件族：`SuperButton`、`SuperText`、`SuperToggle`、`SuperToggleItem`、`SuperInputField`、`SuperDropdown`、`SuperPageScrollView`、`SuperImage`。
- 新增循环列表组件：`UILoopScroll`、`UILoopScrollMul`、`UILoopScrollAndToggle`（先提供框架内可编译、可用的基础实现，不强依赖外部第三方插件）。
- 扩展 `Assets/Editor/UIScriptGenerator`：让生成器识别并为 Super 组件生成对应的绑定/事件模板代码。
- 保持现有 `Button/Toggle/Slider` 生成行为不回归，并兼容对子类组件的事件绑定。

## Capabilities

### New Capabilities
- `super-ui-runtime-components`: 提供 Super 系列 UI 组件与循环列表组件的运行时能力（可挂载、可绑定、可调用）。
- `ui-script-generator-super-integration`: 在现有 UIScriptGenerator 中新增 Super 组件的代码生成与事件绑定规则。

### Modified Capabilities
- None

## Impact

- 受影响代码：
  - `Assets/GameScripts/HotFix/GameLogic/UXTools/Runtime/UXGUI/Components/*`（新增 Super 组件）
  - `Assets/GameScripts/HotFix/GameLogic/UXTools/Runtime/UXGUI/Components/LoopScroll/*`（新增循环列表组件）
  - `Assets/Editor/UIScriptGenerator/UICopyEditor.cs`
  - `Assets/Editor/UIScriptGenerator/UIControlTypeResolver.cs`（若需要补充类型映射策略）
- 受影响流程：UI 组件扫描、`_Auto/_View/_UGUINodeProvider` 生成、运行时 UI 交互逻辑。
- 风险点：若直接照搬外部实现会引入缺失依赖（如 `BaseUIWidget`/第三方 LoopScrollRect）；本变更采用本框架可落地的等价适配实现。
