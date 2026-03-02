#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace UnityFramework.Editor
{
    public static class WidgetGenerator
    {
        private const string UiLayerName = "UI";
        private const float DefaultLocalZ = 0f;

        public static GameObject CreateUIObj(string name)
        {
            GameObject obj = new GameObject(name);
            obj.layer = LayerMask.NameToLayer(UiLayerName);
            RectTransform rectTransform = obj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(100f, 100f);
            obj.transform.localPosition = new Vector3(0f, 0f, DefaultLocalZ);
            return obj;
        }

        public static GameObject CreateUIObj(string name, Vector3 pos, Vector3 size, GameObject[] selection)
        {
            string objName = "UX" + name;
            GameObject obj = new GameObject(objName);
            Undo.RegisterCreatedObjectUndo(obj, string.Empty);
            obj.layer = LayerMask.NameToLayer(UiLayerName);

            Transform parent = FindContainerLogic.GetObjectParent(selection);
            Undo.SetTransformParent(obj.transform, parent, string.Empty);
            obj.transform.SetParent(parent);

            RectTransform rectTransform = Undo.AddComponent<RectTransform>(obj);
            rectTransform.sizeDelta = size;
            obj.transform.localPosition = pos;
            obj.transform.localScale = Vector3.one;
            Undo.SetCurrentGroupName("Create " + objName);
            return obj;
        }
    }
}
#endif
