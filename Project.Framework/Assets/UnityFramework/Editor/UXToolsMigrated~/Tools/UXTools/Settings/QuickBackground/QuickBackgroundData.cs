#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFramework.Editor
{
    [Serializable]
    public class QuickBackgroundData
    {
        public List<QuickBackgroundDataSingle> list = new List<QuickBackgroundDataSingle>();
    }

    [Serializable]
    public class QuickBackgroundDataSingle
    {
        public string name;
        public string guid;
        public QuickBackgroundDetail detail = new QuickBackgroundDetail();
    }

    [Serializable]
    public class QuickBackgroundDetail
    {
        public bool isOpen = false;
        public Vector3 position = default;
        public Vector3 rotation = default;
        public Vector3 scale = Vector3.one;
        public Vector2 size = new Vector2(1920f, 1080f);
        public Color color = Color.white;
        public string spriteId = null;
    }
}
#endif
