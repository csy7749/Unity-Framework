## Context

当前 `Assets/UXTools` 同时包含三类内容：
- Editor 工具（组件库、最近打开、最近选中、检查工具等）
- 运行时 UGUI 组件与辅助逻辑（`UXImage/UXText/UXToggle` 等）
- 与资源路径、代码生成、MVVM 绑定相关的接入代码

项目目标是让功能职责与程序集边界一致：
- Editor-only 功能由 `UnityFramework.Editor` 统一承载与暴露入口；
- 需要热更与未来扩展的 UGUI 运行时能力由 `GameLogic` 侧承接；
- 保持编译边界明确，避免 Runtime 代码直接依赖 `UnityEditor`。

## Goals / Non-Goals

**Goals:**
- 建立可执行的 UXTools 功能拆分规则（按功能、按边界、按依赖）。
- 将 Editor 功能入口归拢到 `UnityFramework/UXTools/*` 菜单域，作为统一访问入口。
- 为 GameLogic 侧建立 UX 组件接入能力（程序集依赖、生成规则、绑定代理）。
- 降低后续拆分成本，确保迁移可分批推进并可验证。

**Non-Goals:**
- 本阶段不强制一次性迁移 UXTools 全量源码到新目录。
- 本阶段不改动业务 UI Prefab 视觉与交互行为。
- 本阶段不引入新的第三方 UI 框架或替换现有 MVVM 架构。

## Decisions

### 决策 1：采用“功能托管 + 渐进迁移”而非“一次性物理搬迁”
- 方案：先在 `UnityFramework.Editor` 与 `GameLogic` 建立可用入口和接入层，再分批迁移源码。
- 原因：一次性搬迁风险高、冲突面大、回滚成本高。
- 备选方案：
  - A. 一次性移动全部文件：回归范围过大，不采用。
  - B. 仅改引用不改结构：边界仍混乱，不采用。

### 决策 2：Editor 功能统一由 UnityFramework.Editor 暴露
- 方案：新增 UnityFramework 菜单入口，托管组件库/最近打开/最近选中等工具访问。
- 原因：满足“Editor 能力集中管理”，同时不阻塞后续源码下沉或替换。
- 备选方案：
  - A. 保留 UXTools 原菜单不变：入口分散，不采用。

### 决策 3：UGUI 运行时能力通过 GameLogic 接入层承接
- 方案：GameLogic 引用 UXTools Runtime，并同步扩展 UI 生成规则与绑定代理。
- 原因：热更侧需要可扩展的组件接入能力，且要避免每次手工绑定。
- 备选方案：
  - A. 仅在主工程运行时使用 UX 组件：无法满足热更扩展诉求，不采用。

### 决策 4：编译边界硬约束
- 方案：Runtime 中 `UnityEditor` 引用必须受 `#if UNITY_EDITOR` 或 Editor 文件隔离。
- 原因：避免 Player/热更编译阶段出现跨边界依赖错误。
- 备选方案：
  - A. 保持现状依赖：后续构建风险不可控，不采用。

## Risks / Trade-offs

- [风险] UXTools 历史代码存在隐式依赖（路径、静态状态、工具配置）  
  → Mitigation：按功能包分批迁移，每批提供编译与行为验收点。

- [风险] UI 代码生成器规则变更可能影响既有脚本生成结果  
  → Mitigation：新增规则采用前缀隔离（如 `m_ux*`），不覆盖旧规则。

- [风险] 热更侧接入新组件后，MVVM 代理反射映射不完整  
  → Mitigation：为非继承链兼容组件（如 `UXToggle`）补充显式代理注册与最小回归。

- [风险] 大规模物理迁移导致 merge 冲突高发  
  → Mitigation：先完成 spec 约束与分阶段任务拆分，按任务批次推进。

## Migration Plan

1. 建立规范与拆分清单（本 change 的 spec 与 tasks）。
2. 完成接入层：程序集依赖、UnityFramework 菜单托管、GameLogic 生成/绑定能力。
3. 以功能包为单位迁移源码：
   - Editor 包（组件库、最近打开、最近选中、检查器工具）。
   - Runtime 包（UGUI 组件、本地化与状态动画相关运行时逻辑）。
4. 每批迁移后执行编译与关键路径回归，逐步收敛遗留 UXTools 目录。

## Open Questions

- “全部代码拆分”是否要求最终删除 `Assets/UXTools` 根目录，还是允许保留过渡层至下一迭代？
- 哪些 Editor 子功能必须首批迁移（除组件库/最近打开/最近选中之外）？
- 是否要求同步调整资源目录（`Assets/UXTools/Res`）归属策略？
