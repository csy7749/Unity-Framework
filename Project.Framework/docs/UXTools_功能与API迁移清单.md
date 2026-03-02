# UXTools 功能与 API 迁移清单

> 基于目录 `Assets/UXTools` 当前代码整理，用于后续功能迁移与入口映射。  
> 关联 OpenSpec Change：`split-uxtools-by-function`。

## 0. OpenSpec 对齐

- 任务项：`openspec/changes/split-uxtools-by-function/tasks.md` 中 `1.1 输出 UXTools 全量功能清单` 仍未勾选。
- 本文可作为该任务的基线文档输入。

## 1. 组件库 / Prefab 资产管理

### 1.1 组件库总窗口
- 功能：组件浏览、筛选、标签管理、拖拽实例化、打包/解包。
- API：
  - `WidgetRepositoryWindow.OpenWindow()`
  - `WidgetRepositoryWindow.GetInstance()`
  - `WidgetRepositoryWindow.RefreshWindow()`
  - `WidgetRepositoryWindow.UnpackPrefab(bool isUnpack, GameObject currentPrefab)`
- 文件：`Assets/UXTools/Editor/Tools/UXTools/Window_Custom/PrefabRepository/WidgetRepositoryWindow.cs`

### 1.2 设为组件 (Set As UXWidget)
- 功能：把当前对象/Prefab设置为UXWidget资产。
- API：
  - `PrefabCreateWindow.OpenWindow()`
  - `PrefabCreateWindow.OpenWindowFromObjList(GameObject[] objList)`
  - `PrefabCreateWindow.OpenWindowFromPrefab(GameObject obj, string path)`
- 菜单：`Assets/设置为组件 (Set As UXWidget)`
- 文件：`Assets/UXTools/Editor/Tools/UXTools/Window_Custom/PrefabRepository/PrefabCreateWindow.cs`

### 1.3 最近打开 Prefab
- 功能：最近打开模板列表展示和快速打开。
- API：
  - `PrefabRecentWindow.OpenWindow()`
  - `PrefabRecentWindow.RefreshWindow()`
  - `PrefabRecentWindow.GetInstance()`
- 文件：`Assets/UXTools/Editor/Tools/UXTools/Window_Custom/PrefabRepository/PrefabRecentWindow.cs`

### 1.4 最近选中文件
- 功能：最近选中资源记录与回跳。
- API：
  - `RecentFilesWindow.ShowWindow()`
  - `RecentFilesWindow.RefreshWindow()`
  - `RecentFilesWindow.GetInstance()`
- 文件：`Assets/UXTools/Editor/Tools/UXTools/Window_Custom/RecentSelectedFiles/RecentFilesWindow.cs`

## 2. 层级管理 / SceneView 工具栏 / 对齐组合

### 2.1 Hierarchy 管理
- 功能：层级规则、分类、搜索、保存。
- API：
  - `HierarchyManagementWindow.OpenHierarchyManage()`
  - `HierarchyManagementWindow.OpenWindow(bool isDemo, Action OnSaveAction = null)`
  - `HierarchyManagementWindow.OpenWindow()`
  - `HierarchyManagementWindow.GetInstance()`
  - `HierarchyManagementWindow.Search(string s, VisualElement resultDiv)`
  - `HierarchyManagementWindow.CloseWindow()`
- 文件：`Assets/UXTools/Editor/Tools/UXTools/Window_Custom/HierarchyManagement/HierarchyManagementWindow.cs`

### 2.2 SceneView 工具栏
- 功能：SceneView 悬浮工具条、快捷创建、更多菜单、参考线菜单。
- API：
  - `SceneViewToolBar.InitFunction()`
  - `SceneViewToolBar.OpenEditor()`
  - `SceneViewToolBar.CloseEditor()`
  - `SceneViewToolBar.SwitchEditor()`
  - `SceneViewToolBar.ShowMorePopUp()`
  - `SceneViewToolBar.ShowGuideLinePopUp()`
  - `SceneViewToolBar.StartQuickCreate(string type)`
  - `SceneViewToolBar.StopQuickCreate()`
- 菜单：`ThunderFireUXTool/工具栏 (Toolbar)`
- 文件：`Assets/UXTools/Editor/Tools/UXTools/Window_Editor/SceneView/SceneViewToolBar.cs`

### 2.3 参考线与吸附
- 功能：创建水平/垂直参考线，吸附、保存、恢复。
- API：
  - `LocationLineLogic.CreateLocationLine(CreateLineType createType)`
  - `LocationLineLogic.RemoveLine(LocationLine line)`
  - `LocationLineLogic.ModifyLine(LocationLine line)`
  - `LocationLineLogic.SnapToFinalPos()`
  - `LocationLineLogic.CheckPosInLines(Vector2 pos, out LocationLine clickLine)`
- 文件：`Assets/UXTools/Editor/Tools/UXTools/Logic/LocationLineLogic/LocationLineLogic.cs`

### 2.4 对齐与网格分布
- 功能：选中 UI 节点对齐/均分。
- API：
  - `AlignLogic.Align(AlignType type)`
  - `AlignLogic.Grid(GridType type)`
  - `AlignLogic.CanAlign(List<RectTransform> rects = null)`
  - `AlignLogic.CanGrid(List<RectTransform> rects = null)`
- 文件：`Assets/UXTools/Editor/Tools/UXTools/Logic/AlignLogic/AlignLogic.cs`

### 2.5 组合节点
- 功能：多节点组合为统一根节点。
- API：
  - `CombineWidgetLogic.GenCombineRootRect(List<RectTransform> rects)`
  - `CombineWidgetLogic.GenCombineRootRect(GameObject[] objs)`
  - `CombineWidgetLogic.CanCombine(GameObject[] objs)`
- 文件：`Assets/UXTools/Editor/Tools/UXTools/Logic/CombineLogic/CombineWidgetLogic.cs`

## 3. 设置 / 欢迎页 / 全局开关

### 3.1 设置窗口
- API：
  - `ConfigurationWindow.OpenWindow()`
  - `ConfigurationWindow.CloseWindow()`
- 菜单：`ThunderFireUXTool/设置 (Setting)`
- 文件：`Assets/UXTools/Editor/Tools/UXTools/Window_Custom/Configuration/ConfigurationWindow.cs`

### 3.2 欢迎页
- API：
  - `GuideWindow.OpenWindow()`
  - `GuideWindow.GetInstance()`
  - `GuideWindow.CloseWindow()`
- 菜单：`ThunderFireUXTool/欢迎页 (Welcome Page)`
- 文件：`Assets/UXTools/Editor/Tools/UXTools/Window_Custom/Guide/GuideWindow.cs`

### 3.3 功能开关
- 功能：统一开关控制（最近打开、吸附、快捷背景、资源检查开关、GamePad 引导等）。
- API：
  - `SwitchSetting.Create()`
  - `SwitchSetting.ChangeSwitch(Toggle[] toggles)`
  - `SwitchSetting.CheckValid(int x)`
  - `SwitchSetting.CheckValid(SwitchType type)`
- 文件：`Assets/UXTools/Editor/Tools/UXTools/Settings/SwitchSetting.cs`

### 3.4 输入系统宏开关
- API：
  - `ScriptingDefineSymbolUtils.EnableInputSystemDefineSymbol()`
  - `ScriptingDefineSymbolUtils.DisableInputSystemDefineSymbol()`
- 文件：`Assets/UXTools/Editor/Tools/UXTools/Utils/ScriptingDefineSymbolUtils.cs`

### 3.5 一键初始化配置资产
- API：`UXToolsAssetsCreator.CreateAllAssets()`
- 菜单：`ThunderFireUXTool/新建配置文件 (Create Assets)/Create All Assets`
- 文件：`Assets/UXTools/Editor/Tools/UXTools/UXToolsAssetsCreator.cs`

## 4. 本地化工具链

### 4.1 本地化设置窗口
- API：
  - `LocalizationSettingWindow.OpenWindow()`
  - `LocalizationSettingWindow.CloseWindow()`
- 菜单：`ThunderFireUXTool/Localization/设置 (Setting)`
- 文件：`Assets/UXTools/Editor/UXGUI/Localization/LocalizationSettingWindow.cs`

### 4.2 文本表创建/同步/导出
- API：
  - `UXTextTable.CreateRuntimeTable(bool alwaysShowPanel = false)`
  - `UXTextTable.CreatePreviewTable(bool alwaysShowPanel = false)`
  - `UXTextTable.OpenTextTable()`
  - `UXTextTable.OpenPreviewTextTable()`
  - `UXTextTable.ReadRow(string key)`
  - `UXTextTable.SyncTextTable()`
- 菜单：
  - `Open Runtime-Use Text Table`
  - `Open Preview Text Table`
  - `Convert Text Table to JSON`
  - `Refresh Runtime-Use Text Table`
- 文件：`Assets/UXTools/Editor/UXGUI/Localization/UXTextTable.cs`

### 4.3 多语言图片导入
- 功能：按语言后缀导入图片到本地化目录。
- 菜单：`Import Localization Images`
- 文件：`Assets/UXTools/Editor/UXGUI/Localization/UXImageImporter.cs`

### 4.4 运行时本地化接口
- API：
  - `LocalizationHelper.ReadFromJSON()`
  - `LocalizationHelper.GetLanguage()`
  - `LocalizationHelper.SetPreviewLanguage(LanguageType type)`
  - `LocalizationHelper.SetLanguage(LanguageType type)`
  - `LocalizationHelper.SetLanguage(int type)`
- 文件：`Assets/UXTools/Runtime/UXGUI/Components/LocalizationHelper.cs`

## 5. 颜色系统 (UIColor)

### 5.1 颜色配置窗口
- API：`UIColorConfigWindow.ShowObjectWindow()`
- 菜单：`ThunderFireUXTool/颜色配置 (UIColorConfig)`
- 文件：`Assets/UXTools/Editor/Feature/UIColor/UIColorConfigWindow.cs`

### 5.2 颜色/渐变资产生成
- API：
  - `UIColorCreator.CreateColor()`
  - `UIColorCreator.CreateGradient()`
- 文件：`Assets/UXTools/Editor/Tools/UXTools/Common/UIColorCreator.cs`

### 5.3 运行时颜色访问
- API：
  - `UIColorUtils.LoadGamePlayerConfig()`
  - `UIColorUtils.InitRuntimeData()`
  - `UIColorUtils.GetDefColorStr(UIColorGenDef.UIColorConfigDef def)`
  - `UIColorUtils.GetDefColor(UIColorGenDef.UIColorConfigDef def)`
  - `UIColorUtils.GetDefGradient(UIGradientGenDef.UIGradientConfigDef def)`
- 文件：`Assets/UXTools/Runtime/Feature/UIColor/UIColorUtils.cs`

## 6. 新手引导 (UIBeginnerGuide)

### 6.1 编辑器入口
- 功能：给选中节点添加 `UIBeginnerGuideDataList`。
- API：`GuideMenu.AddList()`
- 菜单：`ThunderFireUXTool/创建引导 (Create BeginnerGuide)`
- 文件：`Assets/UXTools/Editor/Feature/UIBeginnerGuideEditor/UIBeginnerDataEditor.cs`

### 6.2 编辑器核心
- API：
  - `UIBeginnerGuideEditor.OpenEditor(UIBeginnerGuideData data, GameObject root)`
  - `UIBeginnerGuideEditor.Save()`
  - `UIBeginnerGuideEditor.CloseEditor()`
  - `UIBeginnerGuideEditor.needSave()`
- 文件：`Assets/UXTools/Editor/Feature/UIBeginnerGuideEditor/UIBeginnerGuideEditor.cs`

### 6.3 运行时引导管理
- API：
  - `UIBeginnerGuideManager.SetGuideID(string id)`
  - `UIBeginnerGuideManager.AddGuideList(UIBeginnerGuideDataList datalist)`
  - `UIBeginnerGuideManager.ClearGuideList()`
  - `UIBeginnerGuideManager.RemoveGuideList(UIBeginnerGuideDataList dataList)`
  - `UIBeginnerGuideManager.ShowGuideList()`
  - `UIBeginnerGuideManager.ShowGuideList(UIBeginnerGuideDataList datalist)`
  - `UIBeginnerGuideManager.ShowGuideList(UIBeginnerGuideDataList datalist, string guideID)`
  - `UIBeginnerGuideManager.FinishGuide(string guideId)`
  - `UIBeginnerGuideManager.FinishGuide()`
- 文件：`Assets/UXTools/Runtime/Feature/UIbeginnerGuide/BeginnerGuideManager/UIBeginnerGuideManager.cs`

### 6.4 引导基类接口
- API：
  - `UIBeginnerGuideBase.Init(UIBeginnerGuideData data)`
  - `UIBeginnerGuideBase.Show()`
  - `UIBeginnerGuideBase.Finish()`
  - `GuideWidgetBase.Init(GuideWidgetData data)`
  - `GuideWidgetBase.GetControlledInstanceIds()`
  - `GuideWidgetBase.Show()`
  - `GuideWidgetBase.Stop()`
- 文件：
  - `Assets/UXTools/Runtime/Feature/UIbeginnerGuide/BeginnerGuideManager/UIBeginnerGuideBase.cs`
  - `Assets/UXTools/Runtime/Feature/UIbeginnerGuide/BeginnerGuideWidget/GuideWidgetBase.cs`

## 7. 红点系统 (Reddot)

### 7.1 红点树与广播
- API：
  - `ReddotManager.RegisterRedDotUI(Reddot reddot)`
  - `ReddotManager.UnRegisterRedDotUI(Reddot reddot)`
  - `ReddotManager.ClearReddotData()`
  - `ReddotManager.SetRedDotData(bool isShown, string path)`
  - `ReddotManager.RefreshShown(string path)`
- 文件：`Assets/UXTools/Runtime/Feature/Reddot/ReddotManager.cs`

### 7.2 红点组件
- API：`Reddot.SetReddotShow(bool isShown)`
- 文件：`Assets/UXTools/Runtime/Feature/Reddot/Reddot.cs`

## 8. 资源检查 / 修复 / 引用依赖分析

### 8.1 资源检查主入口
- API：
  - `UIResCheckUtils.CheckAtlas()`
  - `UIResCheckUtils.ExportAtlasDependencies()`
  - `UIResCheckUtils.GetAtlasSpriteDic(ref Dictionary<string, UnityEngine.Object> atlasSpriteDic)`
- 文件：`Assets/UXTools/Editor/Tools/UXTools/Logic/UICheckerLogic/UIResCheckUtils.cs`

### 8.2 组件检查窗口
- API：
  - `UIComponentCheckWindow.OpenWindow()`
  - `UIComponentCheckWindow.SetSelectedField(FieldInfo fieldInfo)`
  - `UIComponentCheckWindow.RemoveFilter(UIComponentCheckFilterBase filter)`
- 文件：`Assets/UXTools/Editor/Tools/UXTools/Logic/UICheckerLogic/UIComponentCheck/UIComponentCheckWindow.cs`

### 8.3 动画修复入口
- 菜单：
  - `UIAnimRenameFix`
  - `UIAnimHierarchyFix`
- 文件：
  - `Assets/UXTools/Editor/Tools/UXTools/Logic/ResourceRepairLogic/AnimationRenameFixTool.cs`
  - `Assets/UXTools/Editor/Tools/UXTools/Logic/ResourceRepairLogic/AnimationHierachyFixTool.cs`

### 8.4 引用/依赖查找
- 菜单：
  - `Assets/==查找资源的引用== (Find Reference)`
  - `Assets/==查找依赖的资源== (Find Dependence)`
  - `ThunderFireUXTool/资源检查工具/.../查找引用 (Find Reference)`
- 文件：
  - `Assets/UXTools/Editor/Tools/_InHouse/Window_Custom/ReferenceFinder/ReferenceFinderWindow.cs`
  - `Assets/UXTools/Editor/Tools/_InHouse/Window_Custom/DependenceFinder/DependenceFinderWindow.cs`

## 9. 快速背景 / 预览

### 9.1 快速背景图
- API：
  - `QuickBackground.CreateBackGround()`
  - `QuickBackground.SetBackGround()`
  - `QuickBackground.Serialize()`
  - `QuickBackground.Close()`
  - `QuickBackground.Load()`
- 文件：`Assets/UXTools/Editor/Tools/UXTools/Logic/QuickBackGround/QuickBackground.cs`

### 9.2 预览流程
- API：
  - `PreviewLogic.Preview()`
  - `PreviewLogic.ExitPreview()`
  - `PreviewLogic.InitPreviewScene(PrefabStage prefabStage)`
  - `PreviewLogic.ResumeOriginScene()`
- 文件：`Assets/UXTools/Editor/Tools/UXTools/Logic/PreviewLogic/PreviewLogic.cs`

## 10. UXGUI 组件与创建入口

### 10.1 编辑器菜单创建组件
- API：
  - `CreateUXImage(MenuCommand menuCommand)`
  - `CreateUXText(MenuCommand menuCommand)`
  - `CreateUXToggle(MenuCommand menuCommand)`
  - `CreateUXScrollView(MenuCommand menuCommand)`
- 菜单：`GameObject/UI/UXImage|UXText|UXTextMeshPro|UXToggle|UXScrollView`
- 文件：`Assets/UXTools/Editor/UXGUI/Inspector/UXUIEditor.cs`

### 10.2 运行时组件族（迁移白名单建议）
- 类：
  - `UXImage`, `UXText`, `UXTextMeshPro`, `UXToggle`, `UXToggleGroup`
  - `UXScrollRect`, `UXMask`, `UXOutline`
  - `UXRolling`, `LXRolling`, `UXRandomInitializer`
  - `UIValueAdapter`
- 文件：`Assets/UXTools/Runtime/UXGUI/Components/*`

## 11. 屏幕/平台适配

### 11.1 SafeArea / 适配
- API：
  - `UIAdapter.GetSafeArea()`
  - `UIAdapter.Refresh()`
  - `IgnoreUIAdapter.Refresh()`
  - `IgnoreUIAdapter.FillScreen()`
  - `IgnoreUIAdapter.RefreshArea()`
- 文件：
  - `Assets/UXTools/Runtime/Feature/UIAdapter/UIAdapter.cs`
  - `Assets/UXTools/Runtime/Feature/UIAdapter/IgnoreUIAdapter.cs`

### 11.2 平台判定
- API：
  - `Platform.InitPlatformInfo()`
  - `Platform.InitPlatformInfoEarly()`
  - `Platform.InitSteamDeckInfo()`
- 文件：`Assets/UXTools/Runtime/Feature/UIAdapter/Platform.cs`

### 11.3 设备模拟器刷新
- API：`UIDeviceSimulatorChangeController.UpdateDevice()`
- 文件：`Assets/UXTools/Runtime/Feature/UIAdapter/UIDeviceSimulatorChangeController.cs`

## 12. UI 状态动画 (UIStateAnimator)

### 12.1 主控制接口
- API：
  - `UIStateAnimator.Init()`
  - `UIStateAnimator.TryInit()`
  - `UIStateAnimator.GetStateNames()`
  - `UIStateAnimator.GetTriggerName()`
  - `UIStateAnimator.SetState(...)`
  - `UIStateAnimator.SetStateInPreview(...)`
  - `UIStateAnimator.LXPrepareFrame(float deltaTime)`
- 文件：`Assets/UXTools/Runtime/UXGUI/UIStateAnimator/UIStateAnimator.cs`

### 12.2 扩展运行时接口
- API（部分关键）：
  - `SetBranch(...)`
  - `GetDuration(string stateName)`
  - `GetDurationByLayer(string stateName, string layerName)`
  - `Fadein(bool skipAnim = false)`
  - `FadeOut(bool skipAnim = false, Action<GameTimer> callBack = null)`
  - `PauseAll()`
  - `InState(string stateName)`
  - `PrepareFrameAllAliveUIStateAnimators(float deltaTime)`
- 文件：`Assets/UXTools/Runtime/UXGUI/UIStateAnimator/UIStateAnimatorEx.cs`

## 13. 分析埋点与辅助

### 13.1 工具使用埋点
- API：
  - `UXToolUsed.InitUXToolUsed()`
  - `UXToolAnalysis.GetUserID()`
  - `UXToolAnalysis.SendUXToolLog(string funcName = "Test1")`
- 事件常量：`UXToolAnalysisLog.*`（WidgetLibrary/RecentlyOpen/...）
- 文件：
  - `Assets/UXTools/Editor/Analysis/UXToolUsed.cs`
  - `Assets/UXTools/Editor/Analysis/UXToolAnalysis.cs`
  - `Assets/UXTools/Editor/Analysis/UXToolAnalysisLog.cs`

## 14. 迁移接口优先级建议（按 OpenSpec）

- P0（已在 spec 场景明确）：
  - `WidgetRepositoryWindow.OpenWindow()`
  - `PrefabRecentWindow.OpenWindow()`
  - `RecentFilesWindow.ShowWindow()`
- P1：
  - `ConfigurationWindow.OpenWindow()`
  - `SceneViewToolBar.OpenEditor()/SwitchEditor()`
  - `UIResCheckUtils.CheckAtlas()`
- P2：
  - `UIBeginnerGuideManager.*`
  - `UIStateAnimator.*`
  - `UIColorUtils.*`
  - `LocalizationHelper.*`

