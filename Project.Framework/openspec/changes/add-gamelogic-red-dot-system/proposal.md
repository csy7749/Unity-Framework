## Why

当前项目 `GameLogic` 下仅有旧版 `Reddot`（布尔显示）实现，缺少配置驱动、数量型红点、查看型红点、按周期重置和本地持久化能力。为对齐参考框架并支撑后续业务接入，需要在 `Assets/GameScripts/HotFix/GameLogic` 内新增一套可独立运行的红点系统。

## What Changes

- 在 `GameLogic/Common` 下新增 `RedDotNew` 运行时系统，提供树结构与节点管理能力。
- 新增数量型节点与查看型节点，支持父链聚合刷新。
- 新增配置资产类型，支持 `Id -> Path/Type/ShowType/BindRole/UseLocalSave` 映射。
- 新增红点视图基类，提供注册、反注册、数量变化回调与显示策略处理。
- 提供本地存储能力（`PlayerPrefs`）并支持按角色绑定键前缀。
- 保持旧版 `Common/RedNote` 与 `UXTools/Runtime/Feature/Reddot` 不变，避免行为回归。

## Capabilities

### New Capabilities
- `gamelogic-red-dot-runtime`: 在 GameLogic 中提供可配置、可注册、可持久化的红点运行时能力。

### Modified Capabilities
- None

## Impact

- 受影响目录：
  - `Assets/GameScripts/HotFix/GameLogic/Common/RedDotNew/Runtime/`
  - `openspec/changes/add-gamelogic-red-dot-system/`
- 受影响行为：
  - 红点节点初始化、查询、注册与状态刷新。
  - 查看型红点的本地时间戳与周期重置逻辑。
  - UI 红点显示（普通/NEW/数字）基础行为。
