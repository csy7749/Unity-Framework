## ADDED Requirements

### Requirement: GameLogic SHALL support UX runtime UGUI components
系统 MUST 允许热更侧（`GameLogic`）接入并使用 UX 运行时 UGUI 组件能力。

#### Scenario: Runtime assembly reference is configured
- **WHEN** GameLogic 需要使用 UX 运行时组件
- **THEN** `GameLogic` 程序集 MUST 具备对 UX 运行时程序集的有效引用

#### Scenario: Hotfix UI code can bind UX controls
- **WHEN** UI 窗口脚本绑定 `UXImage/UXText/UXToggle` 等控件
- **THEN** 绑定流程 MUST 在 GameLogic 侧可编译并可运行

### Requirement: UI script generation SHALL support UX-prefixed controls
系统 MUST 支持通过命名前缀生成 UX 组件绑定代码，并与既有 UGUI 规则共存。

#### Scenario: UX control prefix maps to UX component type
- **WHEN** 控件命名使用 `m_ux*` 前缀
- **THEN** 代码生成器 MUST 生成对应 UX 组件类型字段与绑定代码

#### Scenario: Legacy prefix mapping remains valid
- **WHEN** 控件命名使用历史 `m_text/m_img/m_btn/m_toggle` 前缀
- **THEN** 代码生成器 MUST 保持原有生成行为不变

### Requirement: MVVM proxy registry SHALL cover non-UGUI-inheritance UX controls
系统 MUST 为不继承 UGUI 对等组件的 UX 控件提供显式代理注册能力。

#### Scenario: UXToggle value binding is available
- **WHEN** ViewModel 绑定 `UXToggle.isOn` 与 `onValueChanged`
- **THEN** 绑定系统 MUST 能解析并建立有效代理

#### Scenario: UXToggleGroup configuration binding is available
- **WHEN** ViewModel 绑定 `UXToggleGroup.allowSwitchOff`
- **THEN** 绑定系统 MUST 能解析并设置目标属性

### Requirement: Runtime code SHALL not introduce editor-only dependency leakage
系统 MUST 防止运行时代码引入未隔离的 Editor 依赖。

#### Scenario: Runtime source references UnityEditor
- **WHEN** 运行时代码需要调用 Editor API
- **THEN** 相关 `using` 和调用 MUST 被 `UNITY_EDITOR` 条件编译包裹或拆分到 Editor 文件

#### Scenario: Player build compilation validates runtime boundary
- **WHEN** 进行 Player 目标编译
- **THEN** 运行时程序集 MUST NOT 因 Editor API 泄漏导致编译失败
