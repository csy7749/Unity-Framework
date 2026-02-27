#if UNITY_EDITOR
using GameLogic;
using UnityEditor;
using UnityEngine;

namespace UnityFramework.Editor.UI
{
    public static class UIAutoGenerateMenu
    {
        [MenuItem("GameObject/ScriptGenerator/GenerateByUIControlData", priority = 20)]
        public static void GenerateByUIControlData()
        {
            if (!TryGetRootControlData(out var rootControlData, out var rootObject))
            {
                return;
            }

            UIAutoGenEditorTools.RefreshHierarchy(rootControlData);
            var allData = UIAutoGenEditorTools.GetAllComponentsInPrefab<UIControlData>(rootObject);
            foreach (var data in allData)
            {
                UICopyEditor.CopyUIByUIControlData(data.gameObject, data, rootControlData);
            }

            UICopyEditor.GenerateAllMonoByData(rootControlData, true);
            Debug.Log("GenerateByUIControlData completed.");
        }

        [MenuItem("GameObject/ScriptGenerator/BindByUIControlData", priority = 21)]
        public static void BindByUIControlData()
        {
            if (!TryGetRootControlData(out var rootControlData, out var rootObject))
            {
                return;
            }

            UICopyEditor.BindingAllUGUINodeProvider(rootObject, rootControlData, rootControlData);
            Debug.Log("BindByUIControlData completed.");
        }

        private static bool TryGetRootControlData(out UIControlData rootControlData, out GameObject rootObject)
        {
            rootControlData = null;
            rootObject = Selection.activeGameObject;
            if (rootObject == null)
            {
                Debug.LogError("Selection is empty.");
                return false;
            }

            rootControlData = rootObject.GetComponent<UIControlData>();
            if (rootControlData == null)
            {
                Debug.LogError("Selected GameObject does not have UIControlData.");
                return false;
            }

            return true;
        }
    }
}
#endif
