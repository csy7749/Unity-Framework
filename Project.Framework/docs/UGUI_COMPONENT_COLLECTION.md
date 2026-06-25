# 基于 GameObjectEditor 的 UGUI 组件收集

本文档描述 `GameObjectEditor` 在 Prefab Stage 中对 UGUI 组件的收集、绑定与持久化流程，作为自动生成 UI 代码与 UGUINodeProvider 的基础。

**适用范围**
- 仅在 Prefab 编辑模式生效。非 Prefab Stage 时，`GameObjectEditor` 只显示默认 Inspector。
- 组件收集依赖 `UIControlData`（运行时代码在 `Assets/GameScripts/HotFix/GameLogic/Module/UIModule/AutoGen/UIControlData.cs`）。

**入口与上下文刷新**
- `GameObjectEditor` 是 `GameObject` 的 `CustomEditor`，`OnEnable` 里注册 `EditorApplication.hierarchyChanged`。
- 每次层级变更时会调用 `UIAutoGenEditorTools.RefreshHierarchy` 并刷新当前上下文。
- 上下文包含根节点 `UIControlData`、当前节点 `UIControlData`、父节点 `UIControlData` 以及当前节点可收集菜单项。

**核心数据结构**
- `UIControlData`
  - 生成类型 `GenerateType`：`Window`、`SubItem`、`LoopSubItem`、`None`。
  - 命名：`ClassName`（生成类名）、`VariableName`（字段名）。
  - 控件列表：`CtrlItemDatas`（收集的组件/物体）。
  - 子项列表：`SubUIItemDatas`（子 UI 数据）。
- `CtrlItemData`
  - `name`：字段名。
  - `type`：类型名（组件类型名或 `GameObject`）。
  - `targets`：对象数组，实际只使用第一个元素。
- `SubUIItemData`
  - `subUIData`：指向子 `UIControlData`。

**收集菜单来源（UGUI 组件列表）**
- `UIControlTypeResolver.CollectMenuInfos` 会为当前 `GameObject` 生成可收集条目：
  - 一条 `GameObject` 条目。
  - 该物体上除 `Transform` 和 `UIControlData` 之外的所有组件条目。
- `UIControlTypeResolver` 预置了常见 UGUI 类型映射（`Text`、`Image`、`Button`、`ScrollRect` 等），并会扫描已加载程序集，把所有 `Component` 类型加入映射表。
- 条目生成时会基于物体名生成字段名：`UIAutoGenEditorTools.GetVariableName(name, PascalCase)`。

**收集与生成流程**
- 根节点生成（Prefab 根物体）
  - 选中 Prefab 根物体，点击 `generate view mono script`，会添加 `UIControlData` 并设为 `Window`。
- 子项生成（普通子物体）
  - 选中子物体，点击 `generate item mono script`，会添加 `UIControlData` 并设为 `SubItem`。
- 组件收集菜单（Inspector 中的 `Menu` 折叠区）
  - 若当前物体没有 `UIControlData`，会使用最近的父级 `UIControlData` 作为目标。
  - “+” 会把组件条目写入目标 `UIControlData.CtrlItemDatas`。
  - “-” 会从目标 `UIControlData.CtrlItemDatas` 移除。
- 一键生成
  - `GenerateAll` 会刷新层级、序列化到 Scriptable、复制 UI 数据并生成代码模板。
  - 生成模板时会生成 `ClassName_Auto`、`ClassName_View` 以及 `ClassName_UGUINodeProvider`。

**绑定（UGUINodeProvider）**
- 绑定按钮仅在检测到“需要绑定”时可用，判断条件包括：
  - `UGUINodeProvider` 组件不存在。
  - Provider 公有字段存在空引用。
  - 任意子项未完成绑定。
- 绑定过程会把 `UIControlData` 替换成 `ClassName_UGUINodeProvider` 组件，并按名称进行字段绑定。
- 绑定规则：
  - 字段名与 `CtrlItemData.name` 匹配时，将 `targets[0]` 赋给字段。
  - 若字段类型为 `Transform` 且目标是 `GameObject`，自动取 `transform`。
  - 子项字段会绑定对应的 `SubUIItemData.subUIData`。

**命名与层级刷新规则**
- 当 `ClassName` 或 `VariableName` 为空时，会自动用 `UIAutoGenEditorTools.GetVariableName` 填充。
- `RefreshHierarchy` 会清空并重建子项关系：
  - 通过父级 `UIControlData` 重新挂接 `SubUIItemData`。
  - `GenerateType` 为 `None` 或 `LoopSubItem` 时不会被挂到父级子列表。
  - 组件条目会根据目标物体重新附着到最近的 `UIControlData`。

**持久化（UIControlDataScriptable）**
- 序列化会保存 `UIControlData` 树到 Scriptable：
  - 数据库根路径来自 `ScriptGeneratorSetting.GetScriptableDatabaseRoot()`，默认 `Assets/Editor/UIScriptGenerator/Databases`。
  - 每个 UI 生成一个 `ClassName.asset`。
- 反序列化会从 Scriptable 恢复 `UIControlData` 并重新刷新层级。

**关键实现位置**
- `Assets/Editor/UIScriptGenerator/GameObjectEditor.cs`
- `Assets/Editor/UIScriptGenerator/UIControlTypeResolver.cs`
- `Assets/Editor/UIScriptGenerator/UIAutoGenEditorTools.cs`
- `Assets/Editor/UIScriptGenerator/UICopyEditor.cs`
- `Assets/Editor/UIScriptGenerator/UIControlDataScriptableUtil.cs`
- `Assets/GameScripts/HotFix/GameLogic/Module/UIModule/AutoGen/UIControlData.cs`
