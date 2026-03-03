## ADDED Requirements

### Requirement: GameLogic red dot runtime MUST build and manage path-based node tree
系统 MUST 在 `GameLogic` 内提供路径树能力，可按路径创建和查询红点节点，并维护父子关系。

#### Scenario: Initialize non-parameterized nodes from config
- **WHEN** 首次访问红点树实例且配置中存在不含 `{}` 的路径
- **THEN** 系统 MUST 基于配置初始化对应节点并可被后续查询

#### Scenario: Get node by full path
- **WHEN** 调用方使用完整路径查询节点
- **THEN** 系统 MUST 返回对应节点；路径不存在时 MUST 返回空

### Requirement: Runtime MUST support number node and view node behaviors
系统 MUST 支持数量型与查看型两类叶子节点，并由父节点聚合子节点状态。

#### Scenario: Number node updates and propagates count
- **WHEN** 调用 `ChangeRedDotCount` 或 `ChangeRedDotCountByAccumulation` 修改数量型叶子
- **THEN** 节点数量 MUST 更新且父链聚合值 MUST 同步刷新

#### Scenario: View node toggles unseen/seen state
- **WHEN** 调用 `SetWaitWatch` 设置待查看或调用 `Watch` 标记已查看
- **THEN** 查看型节点 MUST 在未看时返回 1、已看时返回 0，并驱动父链刷新

### Requirement: Runtime MUST provide callback registration lifecycle
系统 MUST 支持回调注册与反注册，并在注册成功后立即推送当前值。

#### Scenario: Register callback receives current value immediately
- **WHEN** 调用 `Register` 并成功绑定节点
- **THEN** 回调 MUST 立即收到当前红点值

#### Scenario: Unregister stops future updates
- **WHEN** 调用 `Unregister` 解除绑定
- **THEN** 后续节点变化 MUST 不再触发该回调

### Requirement: Runtime MUST support local persistence with optional role binding
系统 MUST 提供本地存储能力，并在 `bindRole=true` 时使用角色前缀键。

#### Scenario: Persist number node count when local save enabled
- **WHEN** 配置 `UseLocalSave=true` 且数量节点发生变化
- **THEN** 系统 MUST 将最新数量写入本地存储并可在初始化时恢复

#### Scenario: View reset follows configured period
- **WHEN** 查看型节点存在历史时间戳且周期配置为 `Once/Day/Week/Month`
- **THEN** 系统 MUST 按配置规则决定是否重新置为待查看

### Requirement: Runtime MUST provide reusable UI red dot view base
系统 MUST 提供通用视图基类，根据配置展示 `Normal/New/Number` 三种样式。

#### Scenario: Auto register static path red dot view
- **WHEN** 视图基类初始化且配置路径不含参数占位符
- **THEN** 系统 MUST 自动注册回调并按 `ShowType` 切换显示对象

#### Scenario: Register dynamic path red dot view
- **WHEN** 调用视图基类 `Register(redDotId, params)` 传入路径参数
- **THEN** 系统 MUST 格式化路径并完成注册、显示刷新与反注册回收
