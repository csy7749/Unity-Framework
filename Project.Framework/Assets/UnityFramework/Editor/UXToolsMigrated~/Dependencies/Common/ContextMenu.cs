#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityFramework.Editor
{
    public static class ContextMenu
    {
        private static readonly List<string> Entries = new List<string>();
        private static GenericMenu menu;

        public static void AddItem(string item, bool isChecked, GenericMenu.MenuFunction callback)
        {
            if (callback == null)
            {
                AddDisabledItem(item);
                return;
            }

            if (menu == null)
            {
                menu = new GenericMenu();
            }

            int count = 0;
            for (int i = 0; i < Entries.Count; i++)
            {
                if (Entries[i] == item)
                {
                    count++;
                }
            }

            Entries.Add(item);
            if (count > 0)
            {
                item += " [" + count + "]";
            }

            menu.AddItem(new GUIContent(item), isChecked, callback);
        }

        public static void AddItemWithArge(string item, bool isChecked, GenericMenu.MenuFunction2 callback, object arge)
        {
            if (callback == null)
            {
                AddDisabledItem(item);
                return;
            }

            if (menu == null)
            {
                menu = new GenericMenu();
            }

            int count = 0;
            for (int i = 0; i < Entries.Count; i++)
            {
                if (Entries[i] == item)
                {
                    count++;
                }
            }

            Entries.Add(item);
            if (count > 0)
            {
                item += " [" + count + "]";
            }

            menu.AddItem(new GUIContent(item), isChecked, callback, arge);
        }

        public static void Show()
        {
            if (menu == null)
            {
                return;
            }

            menu.ShowAsContext();
            menu = null;
            Entries.Clear();
        }

        public static void Show(Vector2 pos)
        {
            if (menu == null)
            {
                return;
            }

            menu.DropDown(new Rect(pos, Vector2.zero));
            menu = null;
            Entries.Clear();
        }

        public static void AddCommonItems(GameObject[] targets)
        {
            AddItem("复制", false, () =>
            {
                Unsupported.CopyGameObjectsToPasteboard();
            });

            AddItem("粘贴", false, () =>
            {
                Unsupported.PasteGameObjectsFromPasteboard();
            });

            var prefabStage = PrefabStageUtils.GetCurrentPrefabStage();
            if (prefabStage != null && Selection.Contains(prefabStage.prefabContentsRoot))
            {
                AddItem("删除", false, null);
            }
            else
            {
                AddItem("删除", false, () =>
                {
                    Unsupported.DeleteGameObjectSelection();
                });
            }
        }

        public static void AddSeparator(string path)
        {
            if (menu == null)
            {
                menu = new GenericMenu();
            }

            if (Application.platform != RuntimePlatform.OSXEditor)
            {
                menu.AddSeparator(path);
            }
        }

        public static void AddDisabledItem(string item)
        {
            if (menu == null)
            {
                menu = new GenericMenu();
            }

            menu.AddDisabledItem(new GUIContent(item));
        }

        public static bool IsEmpty()
        {
            return menu == null;
        }
    }
}
#endif
