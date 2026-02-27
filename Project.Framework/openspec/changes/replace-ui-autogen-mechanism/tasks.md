## 1. OpenSpec Artifacts

- [x] 1.1 Finalize proposal, design, and specs for `replace-ui-autogen-mechanism`.
- [x] 1.2 Pass `openspec validate replace-ui-autogen-mechanism --strict`.

## 2. Runtime Data Model

- [x] 2.1 Add `UIControlData` model with generation type, ui layer, ctrl and sub ui collections.
- [x] 2.2 Add runtime marker types (`IBindableUI`, control/sub binding attributes).

## 3. Editor Generation Pipeline

- [x] 3.1 Add template-driven generator for `_View.cs` and `_Auto.cs`.
- [x] 3.2 Add provider generator for `*_UGUINodeProvider.cs`.
- [x] 3.3 Add one-click binder for component replacement and field assignment.

## 4. Inspector Workflow

- [x] 4.1 Add GameObject inspector actions for generate and bind.
- [x] 4.2 Add hierarchy refresh and menu-based control collection.

## 5. Persistence

- [x] 5.1 Add ScriptableObject schema for binding metadata.
- [x] 5.2 Add serialize and deserialize utility for metadata round-trip.

## 6. Migration

- [x] 6.1 Redirect old ScriptGenerator entry to new workflow.
- [ ] 6.2 Verify generated files and bound prefabs are consistent.
