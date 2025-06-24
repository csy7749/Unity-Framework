using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFramework
{
    [System.Serializable]
    public class GuideTransformData : GuideWidgetData
    {
        public override string Serialize()
        {
            UpdateTransformData();
            string data = JsonUtility.ToJson(this);
            return data;
        }
    }
}