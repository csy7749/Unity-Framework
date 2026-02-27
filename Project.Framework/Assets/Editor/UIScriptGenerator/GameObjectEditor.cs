#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using GameLogic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UnityFramework.Editor.UI
{
    [CustomEditor(typeof(GameObject))]
    public sealed class GameObjectEditor : UnityEditor.Editor
    {
        private const string GenerateRootLabel = "generate view mono script";
        private const string GenerateItemLabel = "generate item mono script";

        private UnityEditor.Editor _defaultInspector;
        private GameObject _currentTarget;
        private PrefabStage _prefabStage;
        private UIControlData _rootControlData;
        private UIControlData _currentControlData;
        private UIControlData _parentControlData;
        private List<UGUINodeProviderMenuItemInfo> _menuInfos = new List<UGUINodeProviderMenuItemInfo>();
        private bool _needBinding;
        private bool _showMenu = true;
        private bool _showPersistence = true;
        private bool _showDetails = true;

        private void OnEnable()
        {
            CreateDefaultInspector();
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            RefreshContext();
        }

        private void OnDisable()
        {
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            if (_defaultInspector != null)
            {
                DestroyImmediate(_defaultInspector);
                _defaultInspector = null;
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspectorInternal();
            if (_prefabStage == null)
            {
                return;
            }

            DrawGenerationEntries();
            DrawControlDetails();
            DrawOperationButtons();
            DrawControlMenu();
            DrawPersistence();
        }

        private void DrawDefaultInspectorInternal()
        {
            if (_defaultInspector == null)
            {
                CreateDefaultInspector();
            }

            _defaultInspector?.OnInspectorGUI();
        }

        private void CreateDefaultInspector()
        {
            var inspectorType = Type.GetType("UnityEditor.GameObjectInspector, UnityEditor");
            if (inspectorType != null)
            {
                _defaultInspector = CreateEditor(targets, inspectorType);
            }
        }

        private void OnHierarchyChanged()
        {
            if (_rootControlData != null)
            {
                UIAutoGenEditorTools.RefreshHierarchy(_rootControlData);
            }
            RefreshContext();
        }

        private void RefreshContext()
        {
            _currentTarget = target as GameObject;
            _prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (_prefabStage == null || _currentTarget == null)
            {
                _rootControlData = null;
                _currentControlData = null;
                _parentControlData = null;
                _menuInfos.Clear();
                _needBinding = false;
                return;
            }

            _rootControlData = _prefabStage.prefabContentsRoot != null ? _prefabStage.prefabContentsRoot.GetComponent<UIControlData>() : null;
            _currentControlData = _currentTarget.GetComponent<UIControlData>();
            _parentControlData = UIAutoGenEditorTools.FindComponentInParent<UIControlData>(_currentTarget.transform, false);
            _menuInfos = UIControlTypeResolver.CollectMenuInfos(_currentTarget);
            FillDefaultClassNames();
            _needBinding = _rootControlData != null && UICopyEditor.IsBindingAllUGUINodeProvider(_rootControlData);
        }

        private void FillDefaultClassNames()
        {
            if (_rootControlData != null && string.IsNullOrWhiteSpace(_rootControlData.ClassName))
            {
                _rootControlData.ClassName = UIAutoGenEditorTools.GetVariableName(_prefabStage.prefabContentsRoot.name, NamingConvention.PascalCase);
            }

            if (_currentControlData == null || _currentControlData == _rootControlData)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_currentControlData.VariableName))
            {
                _currentControlData.VariableName = UIAutoGenEditorTools.GetVariableName(_currentTarget.name, NamingConvention.PascalCase);
            }

            if (!string.IsNullOrWhiteSpace(_currentControlData.ClassName))
            {
                return;
            }

            var parentName = _parentControlData != null ? _parentControlData.ClassName : _prefabStage.prefabContentsRoot.name;
            _currentControlData.ClassName = UIAutoGenEditorTools.GetVariableName($"{parentName}_{_currentTarget.name}", NamingConvention.PascalCase);
        }

        private void DrawGenerationEntries()
        {
            if (_rootControlData == null && _prefabStage.prefabContentsRoot == _currentTarget)
            {
                if (GUILayout.Button(GenerateRootLabel))
                {
                    _rootControlData = _currentTarget.AddComponent<UIControlData>();
                    _rootControlData.GenerateType = UIAutoGenerateType.Window;
                    _rootControlData.ClassName = UIAutoGenEditorTools.GetVariableName(_currentTarget.name, NamingConvention.PascalCase);
                    SetDirty(_prefabStage.prefabContentsRoot);
                    RefreshContext();
                }
                return;
            }

            if (_rootControlData != null || _prefabStage.prefabContentsRoot == _currentTarget || _currentControlData != null)
            {
                return;
            }

            if (GUILayout.Button(GenerateItemLabel))
            {
                _currentControlData = _currentTarget.AddComponent<UIControlData>();
                _currentControlData.GenerateType = UIAutoGenerateType.SubItem;
                _currentControlData.VariableName = UIAutoGenEditorTools.GetVariableName(_currentTarget.name, NamingConvention.PascalCase);
                var parentName = _parentControlData != null ? _parentControlData.ClassName : _prefabStage.prefabContentsRoot.name;
                _currentControlData.ClassName = UIAutoGenEditorTools.GetVariableName($"{parentName}_{_currentTarget.name}", NamingConvention.PascalCase);
                SetDirty(_prefabStage.prefabContentsRoot);
                RefreshContext();
            }
        }

        private void DrawControlDetails()
        {
            if (_currentControlData == null)
            {
                return;
            }

            _showDetails = EditorGUILayout.Foldout(_showDetails, "HeaderDetail");
            if (!_showDetails)
            {
                return;
            }

            EditorGUI.BeginChangeCheck();
            _currentControlData.GenerateType = (UIAutoGenerateType)EditorGUILayout.EnumPopup("Generate Type", _currentControlData.GenerateType);
            _currentControlData.ClassName = EditorGUILayout.TextField("Generate Class Name", _currentControlData.ClassName);
            if (_currentControlData.GenerateType == UIAutoGenerateType.SubItem || _currentControlData.GenerateType == UIAutoGenerateType.LoopSubItem)
            {
                _currentControlData.VariableName = EditorGUILayout.TextField("Generate Variable Name", _currentControlData.VariableName);
            }

            if (_currentControlData.GenerateType == UIAutoGenerateType.Window)
            {
                _currentControlData.UILayer = (UILayer)EditorGUILayout.EnumPopup("UI Layer", _currentControlData.UILayer);
                _currentControlData.IsAutoCreateCtrl = EditorGUILayout.Toggle("Auto Create Ctrl", _currentControlData.IsAutoCreateCtrl);
                _currentControlData.IsJustOne = EditorGUILayout.Toggle("Only One Instance", _currentControlData.IsJustOne);
            }

            if (EditorGUI.EndChangeCheck())
            {
                SetDirty(_prefabStage.prefabContentsRoot);
                RefreshContext();
            }
        }

        private void DrawOperationButtons()
        {
            if (_rootControlData == null)
            {
                return;
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("一键生成"))
            {
                GenerateAll();
            }

            EditorGUI.BeginDisabledGroup(!_needBinding);
            if (GUILayout.Button("一键绑定"))
            {
                BindAll();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        private void GenerateAll()
        {
            UIAutoGenEditorTools.RefreshHierarchy(_rootControlData);
            UIControlDataScriptableUtil.SerializeToControlDataScriptable(_rootControlData, _prefabStage.assetPath);

            var allData = UIAutoGenEditorTools.GetAllComponentsInPrefab<UIControlData>(_prefabStage.prefabContentsRoot);
            foreach (var controlData in allData)
            {
                UICopyEditor.CopyUIByUIControlData(controlData.gameObject, controlData, _rootControlData);
            }

            UICopyEditor.GenerateAllMonoByData(_rootControlData, true);
            _needBinding = true;
            RefreshContext();
        }

        private void BindAll()
        {
            _rootControlData = UICopyEditor.BindingAllUGUINodeProvider(_prefabStage.prefabContentsRoot, _rootControlData, _rootControlData);
            _needBinding = UICopyEditor.IsBindingAllUGUINodeProvider(_rootControlData);
            RefreshContext();
        }

        private void DrawControlMenu()
        {
            var targetData = _currentControlData ?? _parentControlData;
            if (targetData == null)
            {
                return;
            }

            _showMenu = EditorGUILayout.Foldout(_showMenu, "Menu");
            if (!_showMenu)
            {
                return;
            }

            foreach (var info in _menuInfos)
            {
                DrawMenuEntry(targetData, info);
            }
        }

        private void DrawMenuEntry(UIControlData targetData, UGUINodeProviderMenuItemInfo info)
        {
            var existing = targetData.GetAdded(info.CtrlItemData);
            EditorGUILayout.BeginHorizontal();
            if (existing != null)
            {
                existing.name = EditorGUILayout.TextField(existing.name);
            }
            else
            {
                EditorGUILayout.LabelField(info.CtrlItemData.name);
            }

            EditorGUI.BeginDisabledGroup(existing != null);
            if (GUILayout.Button($"+ {info.TypeName}", GUILayout.Width(100)))
            {
                targetData.AddControlData(info.CtrlItemData);
                SetDirty(_prefabStage.prefabContentsRoot);
                RefreshContext();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(existing == null);
            if (GUILayout.Button($"- {info.TypeName}", GUILayout.Width(100)))
            {
                targetData.RemoveControlData(info.CtrlItemData);
                SetDirty(_prefabStage.prefabContentsRoot);
                RefreshContext();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPersistence()
        {
            if (_rootControlData == null)
            {
                return;
            }

            _showPersistence = EditorGUILayout.Foldout(_showPersistence, "Persistence");
            if (!_showPersistence)
            {
                return;
            }

            if (GUILayout.Button("创建Root绑定持久化数据"))
            {
                UIControlDataScriptableUtil.SerializeToControlDataScriptable(_rootControlData, _prefabStage.assetPath);
            }

            if (GUILayout.Button("读取持久化绑定数据"))
            {
                var assetPath = $"{UIControlDataScriptableUtil.DatabaseRoot}/{_rootControlData.ClassName}.asset";
                var database = AssetDatabase.LoadAssetAtPath<UIControlDataScriptable>(assetPath);
                if (database == null)
                {
                    Debug.LogError($"Database not found: {assetPath}");
                    return;
                }

                UIControlDataScriptableUtil.DeserializeToUIControlData(database, _prefabStage.assetPath);
                RefreshContext();
            }
        }

        public static void SetDirty(GameObject gameObject)
        {
            EditorUtility.SetDirty(gameObject);
#if UNITY_2021_1_OR_NEWER
            var stage = PrefabStageUtility.GetPrefabStage(gameObject);
#else
            var stage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject);
#endif
            if (stage != null)
            {
                EditorSceneManager.MarkSceneDirty(stage.scene);
            }
        }
    }
}
#endif
