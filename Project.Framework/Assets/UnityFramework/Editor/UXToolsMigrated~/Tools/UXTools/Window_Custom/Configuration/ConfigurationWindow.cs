#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityFramework.Editor;

namespace UnityFramework.Editor.UXTools
{
    public class ConfigurationWindow : EditorWindow
    {
        private const int WindowWidth = 650;
        private const int WindowHeight = 430;

        private static ConfigurationWindow c_window;

        private EditorLocalizationSettings _localizationSettings;
        private UXToolCommonData _commonData;
        private Toggle[] _switchToggles;
        private EnumField _languageField;
        private TextField _projectNameField;
        private IntegerField _maxFilesField;
        private IntegerField _maxPrefabsField;

        [MenuItem(ThunderFireUIToolConfig.Menu_Setting, false, -148)]
        public static void OpenWindow()
        {
            c_window = GetWindow<ConfigurationWindow>();
            c_window.minSize = new Vector2(WindowWidth, WindowHeight);
            c_window.maxSize = new Vector2(WindowWidth, WindowHeight);
            c_window.position = new Rect((Screen.currentResolution.width - WindowWidth) * 0.5f, (Screen.currentResolution.height - WindowHeight) * 0.5f, WindowWidth, WindowHeight);
            c_window.titleContent.text = "设置";
        }

        public static void CloseWindow()
        {
            if (c_window != null)
            {
                c_window.Close();
            }
        }

        [UnityEditor.Callbacks.DidReloadScripts(0)]
        private static void OnScriptReload()
        {
            if (HasOpenInstances<ConfigurationWindow>())
            {
                c_window = GetWindow<ConfigurationWindow>();
            }
        }

        private void OnEnable()
        {
            InitData();
            BuildUI();
        }

        private void InitData()
        {
            _localizationSettings = JsonAssetManager.GetAssets<EditorLocalizationSettings>();
            _commonData = AssetDatabase.LoadAssetAtPath<UXToolCommonData>(ThunderFireUIToolConfig.UXToolCommonDataPath) ?? UXToolCommonData.Create();
            int switchCount = Enum.GetValues(typeof(SwitchSetting.SwitchType)).Length;
            _switchToggles = new Toggle[switchCount];
            for (int i = 0; i < switchCount; i++)
            {
                _switchToggles[i] = new Toggle(((SwitchSetting.SwitchType)i).ToString())
                {
                    value = SwitchSetting.CheckValid(i)
                };
            }
        }

        private void BuildUI()
        {
            rootVisualElement.Clear();
            rootVisualElement.style.paddingLeft = 12;
            rootVisualElement.style.paddingRight = 12;
            rootVisualElement.style.paddingTop = 10;
            rootVisualElement.style.paddingBottom = 10;
            rootVisualElement.style.flexDirection = FlexDirection.Column;

            rootVisualElement.Add(CreateHeader());
            rootVisualElement.Add(CreateGeneralSection());
            rootVisualElement.Add(CreateSwitchSection());
            rootVisualElement.Add(CreateFooter());
        }

        private VisualElement CreateHeader()
        {
            Label title = new Label("设置");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 14;
            title.style.marginBottom = 8;
            return title;
        }

        private VisualElement CreateGeneralSection()
        {
            VisualElement container = new VisualElement();
            container.style.marginBottom = 8;

            _languageField = new EnumField("Language", _localizationSettings.LocalType);
            _projectNameField = new TextField("Unity Title") { value = _commonData.CustomUnityWindowTitle };
            _maxFilesField = new IntegerField("Max Recent Files") { value = _commonData.MaxRecentSelectedFiles };
            _maxPrefabsField = new IntegerField("Max Recent Prefabs") { value = _commonData.MaxRecentOpenedPrefabs };

            _projectNameField.style.marginTop = 6;
            _maxFilesField.style.marginTop = 6;
            _maxPrefabsField.style.marginTop = 6;

            container.Add(_languageField);
            container.Add(_projectNameField);
            container.Add(_maxFilesField);
            container.Add(_maxPrefabsField);
            return container;
        }

        private VisualElement CreateSwitchSection()
        {
            Foldout foldout = new Foldout
            {
                text = "Switches",
                value = true
            };
            foldout.style.flexGrow = 1;
            for (int i = 0; i < _switchToggles.Length; i++)
            {
                foldout.Add(_switchToggles[i]);
            }

            return foldout;
        }

        private VisualElement CreateFooter()
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.FlexEnd;
            row.style.marginTop = 10;

            Button cancelButton = new Button(CloseWindow)
            {
                text = "取消"
            };
            Button okButton = new Button(ApplyAndClose)
            {
                text = "确定"
            };
            cancelButton.style.marginRight = 6;

            row.Add(cancelButton);
            row.Add(okButton);
            return row;
        }

        private void ApplyAndClose()
        {
            if (_maxFilesField.value < 1 || _maxPrefabsField.value < 1)
            {
                EditorUtility.DisplayDialog(
                    "messageBox",
                    "Max values must be >= 1.",
                    "确定");
                return;
            }

            _commonData.CustomUnityWindowTitle = _projectNameField.value ?? string.Empty;
            _commonData.MaxRecentSelectedFiles = _maxFilesField.value;
            _commonData.MaxRecentOpenedPrefabs = _maxPrefabsField.value;
            _commonData.Save();
            EditorLocalName selectedLocal = (EditorLocalName)Enum.Parse(typeof(EditorLocalName), _languageField.value.ToString());
            _localizationSettings.ChangeLocalValue(selectedLocal);
            SwitchSetting.ChangeSwitch(_switchToggles);
            CloseWindow();
        }
    }
}
#endif
