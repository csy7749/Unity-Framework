using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityFramework.Editor;


namespace UnityFramework.Editor.UXTools
{
    public class RecentCreateWindow : EditorWindow
    {
        private static RecentCreateWindow m_window;

        private static string m_name = "";
        private static string m_path = "";
        public static void OpenWindow()
        {
            int width = 350;
            int height = 150;
            //m_name = "新建模板";
            m_window = GetWindow<RecentCreateWindow>();
            m_window.minSize = new Vector2(width, height);
            m_window.titleContent.text = "添加Prefab";
            // m_window.position = new Rect((Screen.currentResolution.width - width) / 2, (Screen.currentResolution.height - height) / 2, width, height);
        }

        private void OnEnable()
        {
            m_path = "";
            var root = rootVisualElement;
            var div = UXBuilder.Div(root, new UXBuilderDivStruct()
            {
                style = new UXStyle()
                {
                    paddingBottom = 20,
                    paddingLeft = 5,
                    paddingTop = 20,
                    paddingRight = 5,
                    alignItems = Align.Center,
                    justifyContent = Justify.Center
                }
            });
            var row = UXBuilder.Row(div, new UXBuilderRowStruct()
            {
                align = Align.Center,
                justify = Justify.Center,
                style = new UXStyle() { marginBottom = 15 }
            });
            UXBuilder.Text(row, new UXBuilderTextStruct()
            {
                text = "Prefab名称",
                style = new UXStyle()
                {
                    width = Length.Percent(28),
                    marginRight = Length.Percent(2)
                }
            });
            var input = UXBuilder.Input(row, new UXBuilderInputStruct()
            {
                style = new UXStyle()
                {
                    width = Length.Percent(60)
                },
                onChange = s => m_name = s
            });
            input.value = m_name;

            row = UXBuilder.Row(div, new UXBuilderRowStruct()
            {
                align = Align.Center,
                justify = Justify.Center,
                style = new UXStyle() { marginBottom = 15 }
            });
            UXBuilder.Text(row, new UXBuilderTextStruct()
            {
                text = "Prefab储存位置",
                style = new UXStyle()
                {
                    width = Length.Percent(28),
                    marginRight = Length.Percent(2)
                }
            });
            var upload = UXBuilder.Upload(row, new UXBuilderPathUploadStruct()
            {
                style = new UXStyle()
                {
                    width = Length.Percent(60)
                },
                inputStyle = new UXStyle()
                {
                    width = Length.Percent(80),
                    marginLeft = Length.Percent(0)
                },
                onChange = s => m_path = s,
                openPath = ""
            });
            // upload.value = "新建模板";

            row = UXBuilder.Row(div, new UXBuilderRowStruct()
            {
                justify = Justify.Center,
                align = Align.Center,
            });
            UXBuilder.Button(row, new UXBuilderButtonStruct()
            {
                type = ButtonType.Primary,
                text = "确定",
                OnClick = AddNewPrefab,
                style = new UXStyle() { width = Length.Percent(30), height = 25, fontSize = 14 }
            });
            UXBuilder.Button(row, new UXBuilderButtonStruct()
            {
                text = "取消",
                OnClick = CloseWindow,
                style = new UXStyle() { width = Length.Percent(30), height = 25, fontSize = 14, marginLeft = 20 }
            });
        }

        private void CloseWindow()
        {
            if (m_window != null)
            {
                m_window.Close();
            }
        }

        public static RecentCreateWindow GetInstance()
        {
            return m_window;
        }

        private void AddNewPrefab()
        {
            GameObject panel = new GameObject("");
            panel.AddComponent<CanvasRenderer>();
            RectTransform r = panel.AddComponent<RectTransform>();
            r.sizeDelta = new Vector2(1151, 659);
            UnityEngine.UI.Image image = panel.AddComponent<UnityEngine.UI.Image>();
            panel.name = m_name;
            image.type = UnityEngine.UI.Image.Type.Sliced;
            image.color = new Color32(255, 255, 255, 100);
            string path = m_path;
            if (Directory.Exists(path))
            {
                if (string.IsNullOrEmpty(panel.name))
                {
                    EditorUtility.DisplayDialog("messageBox",
                        "请输入组件类型名称",
                        "确定",
                        "取消");
                    DestroyImmediate(panel);
                }
                else
                {
                    var go = ToolUtils.CreatePrefab(panel, panel.name, path);
                    Utils.OpenPrefab(path + panel.name + ".prefab");
                    DestroyImmediate(panel);
                    if (go != null) CloseWindow();
                }
            }
            else
            {
                EditorUtility.DisplayDialog("messageBox",
                    "Prefab存储位置不存在，请选择Prefab存储位置",
                    "确定",
                    "取消");
                DestroyImmediate(panel);
            }

        }
    }
}