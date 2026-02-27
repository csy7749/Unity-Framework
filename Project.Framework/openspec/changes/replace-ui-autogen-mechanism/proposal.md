## Why

The current UI generation flow only copies code snippets to clipboard and lacks durable binding metadata, repeatable file generation, and prefab-level auto binding. We need to replace it with a UIControlData-driven pipeline aligned with the existing UIWindow/UIWidget lifecycle.

## What Changes

- Replace old ScriptGenerator snippet workflow with UIControlData-driven generation.
- Add template generation for `_View.cs`, `_Auto.cs`, and `*_UGUINodeProvider.cs`.
- Add one-click binding that replaces `UIControlData` with concrete `*_UGUINodeProvider` and assigns references.
- Add ScriptableObject persistence for binding metadata.
- Add GameObject Inspector entry points for generate and bind operations.
- **BREAKING**: `GameObject/ScriptGenerator/*` is no longer the primary workflow. Output changes from clipboard snippets to generated files plus provider binding.

## Capabilities

### New Capabilities
- `ui-autogen-provider`: UIControlData-based code generation, provider generation, prefab binding, and metadata persistence.

### Modified Capabilities
- None.

## Impact

- Affected code: `Assets/Editor/UIScriptGenerator/*` and new runtime/editor UIControlData files.
- Affected assets: UI prefabs will use `*_UGUINodeProvider` components inheriting `UIControlData`.
- Workflow impact: UI developers move from manual paste to one-click generate and bind.