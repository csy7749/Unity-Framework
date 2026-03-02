using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ThunderFireUITool;
using System;

public class LocalizationSettingWindow : EditorWindow
{
    private static LocalizationSettingWindow c_window;
    private static TextElement localizationFolder;
    private static TextElement previewTablePath;
    private static TextElement runtimeTablePath;

    [MenuItem(ThunderFireUIToolConfig.Menu_Localization + "/设置 (Setting)", false, 54)]
    public static void OpenWindow()
    {
        int width = 650;
        int height = 350;
        c_window = GetWindow<LocalizationSettingWindow>();
        c_window.minSize = new Vector2(width, height);
        c_window.position = new Rect((Screen.currentResolution.width - width) / 2, (Screen.currentResolution.height - height) / 2, width, height);
        c_window.titleContent.text = "设置";
    }

    public static void CloseWindow()
    {
        if (c_window != null)
        {
            c_window.Close();
        }
    }

    private void OnEnable()
    {
        InitWindowData();
        InitWindowUI();
    }

    private VisualElement Root;
    private VisualElement rightContainer;
    private VisualElement leftContainer;
    private ConfigurationOption LocalizeOption;
    private ConfigurationOption PathOption;

    private void InitWindowData()
    {
        UXGUIConfig.availableLanguages.Clear();
        UXGUIConfig.availableLanguages.Add(LocalizationLanguage.ChineseSimplifiedIndex);
    }

    private void InitWindowUI()
    {
        VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ThunderFireUIToolConfig.UIBuilderPath + "SettingWindow.uxml");
        Root = visualTree.CloneTree();
        rootVisualElement.Add(Root);

        leftContainer = Root.Q<VisualElement>("LeftContainer");
        rightContainer = Root.Q<VisualElement>("RightContainer");

        Label nameLabel = Root.Q<Label>("Title");
        nameLabel.text = "本地化设置";

        Button confirmBtn = Root.Q<Button>("ConfirmBtn");
        confirmBtn.clicked += ConfirmOnClick;
        confirmBtn.text = "确定";

        Button cancelBtn = Root.Q<Button>("CancelBtn");
        cancelBtn.clicked += CloseWindow;
        cancelBtn.text = "取消";

        leftContainerRefresh();
        LanguageOnClick();
    }

    private void LanguageOnClick()
    {
        leftContainerRefresh();
        LocalizeOption.isSelect();
        rightContainer.Clear();

        TextElement label = new TextElement();
        label.text = "编译器语言固定为：";
        label.style.position = Position.Absolute;
        label.style.left = 50;
        label.style.top = 20;
        label.style.fontSize = 13;
        label.style.color = Color.white;
        rightContainer.Add(label);

        TextElement languageLabel = new TextElement();
        languageLabel.text = LocalizationLanguage.GetLanguage(LocalizationLanguage.ChineseSimplifiedIndex);
        languageLabel.style.position = Position.Absolute;
        languageLabel.style.left = 50;
        languageLabel.style.top = 45;
        languageLabel.style.fontSize = 13;
        languageLabel.style.color = Color.white;
        rightContainer.Add(languageLabel);
    }

    private void PathOnClick()
    {
        leftContainerRefresh();
        PathOption.isSelect();
        rightContainer.Clear();

        TextElement label = new TextElement();
        label.text = "本地化文件夹" + ":";
        label.style.position = Position.Absolute;
        label.style.left = 50;
        label.style.top = 20;
        label.style.fontSize = 13;
        label.style.color = Color.white;
        rightContainer.Add(label);

        localizationFolder = new TextElement();
        localizationFolder.text = UXGUIConfig.LocalizationFolder;
        localizationFolder.style.position = Position.Absolute;
        localizationFolder.style.left = 50;
        localizationFolder.style.top = 40;
        localizationFolder.style.maxWidth = 400;
        localizationFolder.style.fontSize = 13;
        localizationFolder.style.color = Color.white;
        rightContainer.Add(localizationFolder);

        Image mrIcon = new Image();
        mrIcon.style.height = 10;
        mrIcon.image = ToolUtils.GetIcon("More");
        Button MoreBtn = EditorUIUtils.CreateUIEButton("", mrIcon, SelectPath, 30, 20);
        MoreBtn.style.top = 20;
        MoreBtn.style.right = 50;
        MoreBtn.tooltip = "选择路径";
        rightContainer.Add(MoreBtn);

        label = new TextElement();
        label.text = "静态文本表格路径" + ":";
        label.style.position = Position.Absolute;
        label.style.left = 50;
        label.style.top = 80;
        label.style.fontSize = 13;
        label.style.color = Color.white;
        rightContainer.Add(label);

        runtimeTablePath = new TextElement();
        runtimeTablePath.text = UXGUIConfig.RuntimeTablePath;
        runtimeTablePath.style.position = Position.Absolute;
        runtimeTablePath.style.left = 50;
        runtimeTablePath.style.top = 100;
        runtimeTablePath.style.maxWidth = 400;
        runtimeTablePath.style.fontSize = 13;
        runtimeTablePath.style.color = Color.white;
        rightContainer.Add(runtimeTablePath);

        mrIcon = new Image();
        mrIcon.style.height = 10;
        mrIcon.image = ToolUtils.GetIcon("More");
        MoreBtn = EditorUIUtils.CreateUIEButton("", mrIcon, SelectRuntimeFile, 30, 20);
        MoreBtn.style.top = 80;
        MoreBtn.style.right = 50;
        MoreBtn.tooltip = "选择路径";
        rightContainer.Add(MoreBtn);

        label = new TextElement();
        label.text = "动态文本表格路径" + ":";
        label.style.position = Position.Absolute;
        label.style.left = 50;
        label.style.top = 140;
        label.style.fontSize = 13;
        label.style.color = Color.white;
        rightContainer.Add(label);

        previewTablePath = new TextElement();
        previewTablePath.text = UXGUIConfig.PreviewTablePath;
        previewTablePath.style.position = Position.Absolute;
        previewTablePath.style.left = 50;
        previewTablePath.style.top = 160;
        previewTablePath.style.maxWidth = 400;
        previewTablePath.style.fontSize = 13;
        previewTablePath.style.color = Color.white;
        rightContainer.Add(previewTablePath);

        mrIcon = new Image();
        mrIcon.style.height = 10;
        mrIcon.image = ToolUtils.GetIcon("More");
        MoreBtn = EditorUIUtils.CreateUIEButton("", mrIcon, SelectPreviewFile, 30, 20);
        MoreBtn.style.top = 140;
        MoreBtn.style.right = 50;
        MoreBtn.tooltip = "选择路径";
        rightContainer.Add(MoreBtn);

        Button button = EditorUIUtils.CreateUIEButton("生成空的静态文本表格", null, CreateRuntimeTextTable, 400, 20);
        button.style.top = 200;
        button.style.left = 50;
        rightContainer.Add(button);

        button = EditorUIUtils.CreateUIEButton("生成空的动态文本表格", null, CreatePreviewTextTable, 400, 20);
        button.style.top = 220;
        button.style.left = 50;
        rightContainer.Add(button);
    }

    private void CreateRuntimeTextTable()
    {
        UnityEngine.UI.UXTextTable.CreateRuntimeTable(true);
        runtimeTablePath.text = UXGUIConfig.RuntimeTablePath;
    }

    private void CreatePreviewTextTable()
    {
        UnityEngine.UI.UXTextTable.CreatePreviewTable(true);
        previewTablePath.text = UXGUIConfig.PreviewTablePath;
    }

    private void SelectPath()
    {
        localizationFolder.text = Utils.SelectFolder() ?? localizationFolder.text;
    }

    private void SelectRuntimeFile()
    {
        runtimeTablePath.text = Utils.SelectFile() ?? runtimeTablePath.text;
    }

    private void SelectPreviewFile()
    {
        previewTablePath.text = Utils.SelectFile() ?? previewTablePath.text;
    }

    private void leftContainerRefresh()
    {
        leftContainer.Clear();

        LocalizeOption = new ConfigurationOption("语言", LanguageOnClick);
        LocalizeOption.style.top = 0;
        leftContainer.Add(LocalizeOption);

        PathOption = new ConfigurationOption("路径", PathOnClick);
        PathOption.style.top = 40;
        leftContainer.Add(PathOption);
    }

    private void ConfirmOnClick()
    {
        UXGUIConfig.availableLanguages.Clear();
        UXGUIConfig.availableLanguages.Add(LocalizationLanguage.ChineseSimplifiedIndex);
        if (localizationFolder != null)
        {
            UXGUIConfig.LocalizationFolder = localizationFolder.text;
            UXGUIConfig.RuntimeTablePath = runtimeTablePath.text;
            UXGUIConfig.PreviewTablePath = previewTablePath.text;
        }
        else
        {
            var uxConfig = ResourceManager.Load<UXGUIConfig>("UXGUIConfig");
            EditorUtility.SetDirty(uxConfig);
            AssetDatabase.SaveAssets();
        }
        Selection.activeGameObject = null;

        CloseWindow();
    }
}
