## 1. Baseline and boundary inventory

- [ ] 1.1 Output full UXTools feature inventory (Editor vs Runtime vs shared utility) and mark target owner (`UnityFramework.Editor` or `GameLogic`).
- [ ] 1.2 Build dependency graph for UXTools-related asmdef files and identify illegal dependency directions.
- [ ] 1.3 Define migration batch list (component library, recent-opened, recent-selected, UI generator integration, runtime UGUI controls).

## 2. Editor feature hosting in UnityFramework

- [ ] 2.1 Add UnityFramework-level menu entrypoints for component library / recent opened prefabs / recent selected files.
- [ ] 2.2 Move or re-home Editor-only feature modules into UnityFramework-owned editor structure by migration batches.
- [ ] 2.3 Ensure Editor tools compile only under Editor assemblies and keep behavior parity after migration.

## 3. Hotfix UGUI runtime integration in GameLogic

- [ ] 3.1 Establish GameLogic-to-UX runtime assembly integration and verify hotfix compilation path.
- [ ] 3.2 Extend UI script generator mapping to support `m_ux*` prefixes for UX controls without breaking legacy mappings.
- [ ] 3.3 Extend MVVM proxy registrations for UX-specific controls that are not covered by existing UGUI inheritance mappings.

## 4. Runtime boundary hardening

- [ ] 4.1 Refactor runtime files so `UnityEditor` references are isolated by `#if UNITY_EDITOR` or Editor-only files.
- [ ] 4.2 Validate Runtime/Editor assembly boundaries by running Editor compile and Player-target compile checks.
- [ ] 4.3 Remove or isolate obsolete cross-boundary compatibility paths discovered during migration.

## 5. Full split completion and verification

- [ ] 5.1 Complete all planned functional migration batches and update inventory status to 100%.
- [ ] 5.2 Verify critical paths: UnityFramework editor entry usability, GameLogic hotfix UI binding and runtime behavior.
- [ ] 5.3 Produce final migration report (moved features, remaining risks, rollback points) and mark change ready for `/opsx:apply`.
