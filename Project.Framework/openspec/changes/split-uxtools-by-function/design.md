## Context

`Assets/UXTools` 当前混合了 Editor 工具与运行时能力，历史耦合较高。  
本次以“功能拆分”为核心，先迁移 Editor，再处理运行时。  
基线约束：当前版本不包含 super 系列扩展能力，本设计不得引入 `SuperButton/SuperToggle` 及同类新扩展需求。

## Goals / Non-Goals

**Goals:**
- 建立 UXTools 的功能级拆分清单与迁移顺序。
- 将 Editor 功能归拢到 `UnityFramework.Editor` 体系内。
- 保证迁移后程序集边界清晰，避免 Editor/Runtime 依赖泄漏。
- 为后续运行时代码迁移到 `GameLogic` 预留结构化路径。

**Non-Goals:**
- 不在本变更中引入 super 系列组件或其代码生成/绑定扩展。
- 不改变现有业务 UI 行为与资源组织策略。
- 不一次性迁移 UXTools 全部文件；采用批次迁移与逐批验收。

## Decisions

### 决策 1：采用“功能批次迁移”而不是“一次性全搬迁”
- 先做 Editor 三个核心功能（组件库、最近打开、最近选中）及闭包依赖。
- 每批迁移后执行编译与行为回归，再进入下一批。

### 决策 2：统一入口归属到 UnityFramework
- `UnityFramework/UXTools/*` 作为统一入口。
- 老入口可保留过渡，但以新入口为主验证路径。

### 决策 3：运行时迁移仅对齐当前版本能力
- 热更侧只基于现有 UGUI/UXTools 运行时代码做迁移规划。
- 不新增 super 系列映射、代理注册、命名规则。

### 决策 4：严格控制编译边界
- Runtime 代码中 `UnityEditor` 引用必须条件编译或 Editor 文件隔离。
- 避免迁移后出现跨程序集非法依赖。

## Risks / Trade-offs

- [风险] UXTools Editor 内部依赖链长，迁移时易漏文件  
  → Mitigation：按窗口功能做依赖闭包清点并建立迁移清单。

- [风险] 迁移后短期内会出现“新旧并存”  
  → Mitigation：通过统一入口与分阶段清理控制风险。

- [风险] 文档与代码再次偏离  
  → Mitigation：每次回退或重基线时先更新 OpenSpec，再执行迁移。

## Migration Plan

1. 更新 spec 文档到当前版本基线（本次已执行）。
2. 迁移 Editor 第一批：组件库、最近打开、最近选中及依赖。
3. 完成 Editor 第一批编译与行为验证，记录差异。
4. 迁移 Editor 后续批次，逐步收敛 UXTools Editor 目录。
5. 启动运行时迁移批次（不含 super 系列扩展）。

## Open Questions

- Editor 第一批迁移后，老菜单入口保留多久再移除？
- 运行时迁移是否要求同步重组 `Assets/UXTools/Res` 目录？
- 是否需要为每批迁移单独产出回滚脚本？
