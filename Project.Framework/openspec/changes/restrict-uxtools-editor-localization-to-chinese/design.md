## Context

UXTools 在“设置-通用-语言”里提供了多语言切换，相关 UI 文案依赖 `EditorLocalization.GetLocalization`。当前需求为编辑器只保留中文，因此该入口需要收敛为单语言，并改为中文直写。

## Goals / Non-Goals

**Goals:**
- 将设置窗口“通用-语言”改为固定简体中文且不可切换。
- 移除该窗口内 `GetLocalization` 调用，直接使用中文文案。
- 将编辑器语言配置默认值和保存值固定为中文。

**Non-Goals:**
- 不改动运行时国际化模块。
- 不改动 UXGUI 文本表与运行时多语言数据结构。
- 不改动其他窗口的本地化策略。

## Decisions

### 决策 1：语言控件保留但只读
- `ConfigurationWindow` 语言下拉仅保留“简体中文”，并禁用交互。
- 理由：保留原布局与用户认知，最小化改动风险。

### 决策 2：设置窗口文案改为中文直写
- `ConfigurationWindow` 内所有 `EditorLocalization.GetLocalization` 替换为中文字符串常量。
- 理由：直接满足“移除 `GetLocalization` 引用”要求，行为可预期。

### 决策 3：语言配置写入强制中文
- `EditorLocalizationSettings.Create` 默认中文；`ChangeLocalValue` 始终落到中文。
- 理由：防止通过其他调用路径写入非中文值。

## Risks / Trade-offs

- [风险] 其他窗口仍使用 `GetLocalization`，表现与本窗口不一致  
  → Mitigation：本变更仅覆盖用户指定入口，范围在 OpenSpec 中明确。

- [风险] 仅中文后无法直接回切多语言  
  → Mitigation：本变更为需求驱动收敛，回退时可通过 change 回滚。

## Migration Plan

1. 更新 OpenSpec 规格与任务，明确单语言目标和验收标准。
2. 修改 `ConfigurationWindow`：语言控件固定中文，文案中文直写。
3. 修改 `EditorLocalizationSettings`：默认值与保存值固定中文。
3. 本地编译检查并修复编译错误。
4. 手动核对“设置-通用-语言”仅显示中文且不可切换。

## Open Questions

- 是否要在其他设置窗口同步移除 `GetLocalization` 调用。
