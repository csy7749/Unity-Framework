## 1. OpenSpec 与目录准备

- [x] 1.1 创建 change `add-gamelogic-red-dot-system` 并补齐 proposal/design/spec artifacts
- [x] 1.2 在 `Assets/GameScripts/HotFix/GameLogic/Common/RedDotNew/Runtime` 建立运行时代码目录结构

## 2. 运行时核心实现

- [x] 2.1 实现 `RedDotNodeBase`（树初始化、查询、注册反注册、清理）
- [x] 2.2 实现 `RedDotNumberNode`（数量设置、增量设置、父链聚合）
- [x] 2.3 实现 `RedDotViewNode`（查看状态、已看持久化、父链聚合）
- [x] 2.4 实现 `RedDotTree`（配置驱动初始化、Register/Unregister、Change/Watch API、本地存储）
- [x] 2.5 实现 `ViewType` 与 `RedDotShowType` 枚举

## 3. 配置与视图层实现

- [x] 3.1 实现 `RedDotConfigAsset` 与 `RedDotConfigData`（Data/DataDic、初始化）
- [x] 3.2 实现 `RedDotViewBase`（自动/主动注册、显示模式切换、反注册）

## 4. 验证与收尾

- [x] 4.1 扫描新增 API 与命名空间，确认不覆盖旧版红点系统
- [x] 4.2 完成本次变更任务勾选并执行 `openspec validate --strict`
