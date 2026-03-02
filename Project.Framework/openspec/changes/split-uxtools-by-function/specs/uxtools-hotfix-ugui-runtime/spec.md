## ADDED Requirements

### Requirement: GameLogic SHALL integrate runtime UI code based on current version capabilities
系统 MUST 按当前版本已有能力规划并接入运行时 UGUI 相关代码到 `GameLogic`。

#### Scenario: Runtime integration scope is baseline-driven
- **WHEN** 定义运行时迁移范围
- **THEN** 迁移范围 MUST 仅包含当前仓库已有的运行时能力

#### Scenario: Hotfix compile path remains valid
- **WHEN** 完成运行时迁移批次
- **THEN** `GameLogic` 热更编译链路 MUST 保持可编译

### Requirement: Existing UI generation mapping SHALL remain stable in this change
系统 MUST 保持当前版本的 UI 脚本生成映射行为，不在本变更引入 super 系列或新命名前缀扩展。

#### Scenario: Legacy mapping remains unchanged
- **WHEN** 使用现有 `m_text/m_img/m_btn/m_toggle` 等命名规则生成代码
- **THEN** 生成结果 MUST 与当前版本行为一致

#### Scenario: No super-series mapping is introduced
- **WHEN** 执行本 change 的实现
- **THEN** 系统 MUST NOT 新增 `SuperButton/SuperToggle` 或其他 `Super*` 组件映射需求

### Requirement: MVVM binding migration SHALL not add super-series dependencies
系统 MUST 在迁移过程中维持现有 MVVM 绑定能力，不引入 super 系列依赖。

#### Scenario: Existing binding contracts stay available
- **WHEN** 迁移后执行现有 UI 绑定路径
- **THEN** 既有绑定契约 MUST 保持可用

#### Scenario: Super-series dependencies are blocked
- **WHEN** 评审迁移代码
- **THEN** 迁移代码 MUST NOT 包含新增 `Super*` 绑定代理注册

### Requirement: Runtime editor dependency leakage SHALL be prevented
系统 MUST 防止运行时代码引入未隔离的 Editor 依赖。

#### Scenario: Runtime source uses editor API
- **WHEN** 运行时代码需要使用 `UnityEditor` API
- **THEN** 相关代码 MUST 使用 `UNITY_EDITOR` 条件编译或拆分到 Editor 文件

#### Scenario: Player build validates boundary
- **WHEN** 执行 Player 目标编译
- **THEN** 不得因 Editor 依赖泄漏导致运行时程序集编译失败
