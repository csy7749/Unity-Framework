#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityFramework.Editor
{
    public static class CombineWidgetLogic
    {
        public static GameObject GenCombineRootRect(GameObject[] objs)
        {
            List<RectTransform> rects = objs
                .Where(obj => obj != null)
                .Select(obj => obj.GetComponent<RectTransform>())
                .Where(rect => rect != null)
                .ToList();
            return GenCombineRootRect(rects);
        }

        public static GameObject GenCombineRootRect(List<RectTransform> rects)
        {
            if (rects == null || rects.Count == 0)
            {
                return null;
            }

            Transform parent = rects[0].parent;
            Rect bounds = GetBoundsInParent(rects, parent);
            GameObject root = new GameObject("root", typeof(RectTransform));
            root.layer = LayerMask.NameToLayer("UI");
            Undo.RegisterCreatedObjectUndo(root, "Combine Operation");

            RectTransform rootRect = root.GetComponent<RectTransform>();
            Undo.SetTransformParent(root.transform, parent, "Combine Operation");
            root.transform.SetParent(parent, false);
            rootRect.localScale = Vector3.one;
            rootRect.localRotation = Quaternion.identity;
            rootRect.sizeDelta = new Vector2(bounds.width, bounds.height);
            rootRect.anchoredPosition = bounds.center;

            int minSiblingIndex = rects.Min(rect => rect.GetSiblingIndex());
            root.transform.SetSiblingIndex(minSiblingIndex);
            foreach (RectTransform rect in rects.OrderBy(rect => rect.GetSiblingIndex()))
            {
                Undo.SetTransformParent(rect.transform, root.transform, "Combine Operation");
            }

            return root;
        }

        public static bool CanCombine(GameObject[] objs)
        {
            return objs != null &&
                objs.Length > 1 &&
                AllHaveRectTransform(objs) &&
                AllHaveSameParent(objs) &&
                !ContainsPrefabAssetInstance(objs);
        }

        private static bool AllHaveSameParent(GameObject[] objs)
        {
            Transform parent = objs[0].transform.parent;
            return objs.Skip(1).All(obj => obj != null && obj.transform.parent == parent);
        }

        private static bool AllHaveRectTransform(GameObject[] objs)
        {
            return objs.All(obj => obj != null && obj.GetComponent<RectTransform>() != null);
        }

        private static bool ContainsPrefabAssetInstance(GameObject[] objs)
        {
            return objs.Any(obj => !string.IsNullOrEmpty(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj)));
        }

        private static Rect GetBoundsInParent(List<RectTransform> rects, Transform parent)
        {
            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;
            Vector3[] corners = new Vector3[4];

            foreach (RectTransform rect in rects)
            {
                rect.GetWorldCorners(corners);
                for (int i = 0; i < corners.Length; i++)
                {
                    Vector3 local = parent.InverseTransformPoint(corners[i]);
                    minX = Mathf.Min(minX, local.x);
                    minY = Mathf.Min(minY, local.y);
                    maxX = Mathf.Max(maxX, local.x);
                    maxY = Mathf.Max(maxY, local.y);
                }
            }

            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }
    }
}
#endif
