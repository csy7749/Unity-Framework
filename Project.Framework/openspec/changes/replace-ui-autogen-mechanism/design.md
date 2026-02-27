## Context

The project UI lifecycle is based on `UIBase -> UIWindow/UIWidget` and runs binding logic in `ScriptGenerator()`. The current editor tool only generates clipboard snippets. The target mechanism uses `UIControlData` metadata to generate `_View/_Auto`, generate `*_UGUINodeProvider`, and bind prefab fields through typed providers.

## Goals / Non-Goals

**Goals:**
- Introduce `UIControlData` metadata model for controls and sub UIs.
- Provide automatic generation for `_View/_Auto/_UGUINodeProvider`.
- Provide one-click prefab binding with component replacement and field assignment.
- Provide ScriptableObject persistence for binding metadata.
- Keep compatibility with current `UIWindow/UIWidget` lifecycle.

**Non-Goals:**
- Do not migrate business runtime base classes from the reference project.
- Do not import external business component packages.
- Do not redesign existing MVVM runtime binding framework.

## Decisions

- Reuse current project namespace and lifecycle, migrate generation mechanism only.
- Keep `*_UGUINodeProvider : UIControlData` and access controls via typed provider in `_Auto`.
- Store templates in project-local paths instead of package-relative external paths.
- Keep binding explicit: generation then binding; failures are reported as errors.
- Reuse ScriptGenerator settings for namespace/path compatibility where possible.

## Risks / Trade-offs

- [Risk] Provider type is unavailable before compile -> Mitigation: explicit binding errors and retry guidance.
- [Risk] Prefab data loss during component replacement -> Mitigation: clone and restore UIControlData payload.
- [Risk] Mixed usage of old and new tools -> Mitigation: old menu redirects to new workflow.
- [Risk] Nested sub UI reconstruction issues -> Mitigation: refresh hierarchy and rebuild ctrl/sub relations before generation.

## Migration Plan

1. Add UIControlData runtime and editor pipeline.
2. Add UIControlData to prefabs and run one-click generation.
3. Compile scripts and run one-click binding.
4. Incrementally migrate windows to `_View/_Auto` structure.

## Open Questions

- Whether to remove old ScriptGenerator menu completely or keep redirect entry.
- Final default output directory under GameLogic hotfix folder.