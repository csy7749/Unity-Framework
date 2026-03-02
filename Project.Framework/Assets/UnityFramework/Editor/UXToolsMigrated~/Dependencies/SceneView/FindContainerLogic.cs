#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;

namespace UnityFramework.Editor
{
    public static class FindContainerLogic
    {
        // Decide where dragged widgets should be parented in scene/prefab stage.
        public static Transform GetObjectParent(GameObject[] selection)
        {
            var prefabStage = PrefabStageUtils.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                bool singleValidSelection = selection.Length == 1 &&
                    !selection[0].name.Equals("Canvas (Environment)") &&
                    selection[0].transform != prefabStage.prefabContentsRoot.transform;
                return singleValidSelection ? selection[0].transform.parent : prefabStage.prefabContentsRoot.transform;
            }

            if (selection.Length == 1)
            {
                Transform selected = selection[0].transform;
                return selected == selected.root ? selected.root : selected.parent;
            }

            Canvas[] canvases = Object.FindObjectsOfType<Canvas>();
            if (canvases.Length == 0)
            {
                new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvases = Object.FindObjectsOfType<Canvas>();
            }

            return canvases[0].transform;
        }
    }
}
#endif
