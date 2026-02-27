# Repository Guidelines

## Project Structure & Module Organization
- `Assets/UnityFramework/Runtime`: core framework runtime modules.
- `Assets/UnityFramework/Editor`: editor tooling, build automation, and release utilities.
- `Assets/GameScripts/HotFix`: hot-update gameplay code (`GameLogic`, `GameProto` assemblies).
- `Assets/Launcher`: app bootstrap and launch flow.
- `Assets/AssetRaw`, `Assets/AssetArt`, `Assets/Resources`, `Assets/Scenes`: game content and scene assets.
- `Packages/`: Unity packages (for example `YooAsset`, `UniTask`) plus package manifest/lock files.
- `ProjectSettings/`, `Packages/manifest.json`: Unity configuration (project currently uses Unity `2022.3.62f3c1`).
- Generated folders (`Library/`, `Temp/`, `Logs/`, `Build/`, `Builds/`, `Bundles/`) should not be committed.

## Build, Test, and Development Commands
- Open locally in Unity Hub using this folder as the project root.
- Headless AssetBundle build:
  ```powershell
  Unity.exe -projectPath . -batchmode -quit -executeMethod UnityFramework.ReleaseTools.BuildAssetBundle -CustomArgs:platform=Windows;outputRoot=./Bundles;packageVersion=2026.02.26
  ```
- Run EditMode tests:
  ```powershell
  Unity.exe -projectPath . -batchmode -runTests -testPlatform EditMode -testResults Logs/EditModeTests.xml -quit
  ```
- Run PlayMode tests:
  ```powershell
  Unity.exe -projectPath . -batchmode -runTests -testPlatform PlayMode -testResults Logs/PlayModeTests.xml -quit
  ```
- In-Editor shortcuts are under `UnityFramework/Build/*` (for example one-click Windows/Android/iOS builds).

## Coding Style & Naming Conventions
- Use C# conventions already present in the project: 4-space indentation, braces on new lines, UTF-8 text.
- `PascalCase` for types/methods/properties; `camelCase` for locals/parameters; private fields use `_camelCase`.
- Keep runtime/editor code separated (`Runtime` vs `Editor` folders and asmdefs).
- Prefer small, focused classes and keep module boundaries explicit via asmdef dependencies.

## Testing Guidelines
- Unity Test Framework is available (`com.unity.test-framework`).
- Place new tests in dedicated test assemblies (EditMode for pure logic, PlayMode for scene/integration flows).
- Name test files after target behavior, e.g. `SceneModuleTests.cs`, and write descriptive test names.
- No fixed coverage gate is configured; add tests for any bug fix or non-trivial feature.

## Commit & Pull Request Guidelines
- Recent history favors short imperative messages: `Fix ...`, `Update ...`, `Rewrite ...`, `Remove ...`.
- Keep each commit scoped to one change set (logic, assets, tooling, or refactor).
- PRs should include: purpose, key files/modules changed, test/build evidence, and affected platforms.
- Include screenshots or short videos for UI/scene changes, and note asset/bundle impact when relevant.
