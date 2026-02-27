## ADDED Requirements

### Requirement: UIControlData-driven code generation
The system SHALL generate `_View.cs` and `_Auto.cs` from `UIControlData` metadata on UI prefabs, and generation SHALL be repeatable.

#### Scenario: Generate window and sub item files
- **WHEN** a developer triggers one-click generation in prefab editing context
- **THEN** the system generates `_View.cs` and `_Auto.cs` for root window and all sub items

### Requirement: UGUINodeProvider generation
The system SHALL generate `*_UGUINodeProvider.cs` for each `UIControlData`, including control fields and sub UI provider fields.

#### Scenario: Generate provider class
- **WHEN** a developer triggers provider generation
- **THEN** the system writes a provider class inheriting `UIControlData` with all declared binding fields

### Requirement: Prefab auto binding
The system SHALL support replacing prefab `UIControlData` components with concrete `*_UGUINodeProvider` components and assigning references.

#### Scenario: One-click bind
- **WHEN** a developer triggers one-click bind
- **THEN** the system replaces components and writes control/sub UI references into provider fields

### Requirement: Binding metadata persistence
The system SHALL serialize `UIControlData` binding metadata to ScriptableObject assets and SHALL deserialize it back to prefabs.

#### Scenario: Persist and restore metadata
- **WHEN** a developer runs serialize or deserialize actions
- **THEN** the system stores or restores control and sub UI binding metadata

### Requirement: Runtime control access through provider
Generated `_Auto` code SHALL fetch `UIControlData`, cast it to `*_UGUINodeProvider`, and access controls through provider fields.

#### Scenario: Runtime initialization path
- **WHEN** a `UIWindow` or `UIWidget` runs generated initialization
- **THEN** generated members access controls through `_uGUINodeProvider` instead of per-field reflection lookup