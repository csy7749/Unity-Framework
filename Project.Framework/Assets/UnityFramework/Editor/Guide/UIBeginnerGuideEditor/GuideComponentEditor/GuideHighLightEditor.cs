using System;
using UnityEditor;
using UnityEngine;

namespace UnityFramework.Editor
{
    [CustomEditor(typeof(GuideHighLight))]
    public class GuideHighLightEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            GUI.enabled=false;
            base.OnInspectorGUI();
            GUI.enabled=true;

        }
    }
}