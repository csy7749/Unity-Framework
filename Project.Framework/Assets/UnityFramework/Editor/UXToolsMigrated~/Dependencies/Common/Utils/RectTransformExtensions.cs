#if UNITY_EDITOR
using UnityEngine;

namespace UnityFramework.Editor
{
    internal static class RectTransformExtensions
    {
        public static float GetLeftWorldPosition(this RectTransform rect)
        {
            return rect.TransformPoint(new Vector3(rect.rect.xMin, 0f, 0f)).x;
        }

        public static float GetRightWorldPosition(this RectTransform rect)
        {
            return rect.TransformPoint(new Vector3(rect.rect.xMax, 0f, 0f)).x;
        }

        public static float GetTopWorldPosition(this RectTransform rect)
        {
            return rect.TransformPoint(new Vector3(0f, rect.rect.yMax, 0f)).y;
        }

        public static float GetBottomWorldPosition(this RectTransform rect)
        {
            return rect.TransformPoint(new Vector3(0f, rect.rect.yMin, 0f)).y;
        }
    }
}
#endif
