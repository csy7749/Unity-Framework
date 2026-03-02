#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityFramework.Editor;
using UXTools.Editor.Common.Data;

namespace UnityFramework.Editor.UXTools
{
    public class AboutWindow : EditorWindow
    {
        private const int WindowWidth = 480;
        private const int WindowHeight = 274;
        private static AboutWindow m_window;
        private ToolGlobalData _globalData;

        [MenuItem(ThunderFireUIToolConfig.Menu_About, false, -150)]
        public static void OpenWindow()
        {
            m_window = GetWindow<AboutWindow>();
            if (m_window == null)
            {
                return;
            }

            m_window.minSize = new Vector2(WindowWidth, WindowHeight);
            m_window.maxSize = new Vector2(WindowWidth, WindowHeight);
            m_window.titleContent.text = "关于";
            m_window.position = new Rect((Screen.currentResolution.width - WindowWidth) * 0.5f, (Screen.currentResolution.height - WindowHeight) * 0.5f, WindowWidth, WindowHeight);
        }

        private void OnEnable()
        {
            _globalData = JsonAssetManager.GetAssets<ToolGlobalData>();
            DrawUI();
        }

        private void DrawUI()
        {
            rootVisualElement.Clear();
            rootVisualElement.style.paddingLeft = 16;
            rootVisualElement.style.paddingRight = 16;
            rootVisualElement.style.paddingTop = 16;
            rootVisualElement.style.paddingBottom = 16;
            rootVisualElement.style.flexDirection = FlexDirection.Column;
            rootVisualElement.style.justifyContent = Justify.SpaceBetween;

            Label title = new Label("关于");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 18;

            Label version = new Label($"Version: {(_globalData?.Version ?? "unknown")}");
            version.style.marginTop = 8;

            Button closeBtn = new Button(CloseWindow)
            {
                text = "关闭"
            };
            closeBtn.style.alignSelf = Align.FlexEnd;

            rootVisualElement.Add(title);
            rootVisualElement.Add(version);
            VisualElement spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            rootVisualElement.Add(spacer);
            rootVisualElement.Add(closeBtn);
        }

        private static void CloseWindow()
        {
            if (m_window != null)
            {
                m_window.Close();
            }
        }
    }
}
#endif
