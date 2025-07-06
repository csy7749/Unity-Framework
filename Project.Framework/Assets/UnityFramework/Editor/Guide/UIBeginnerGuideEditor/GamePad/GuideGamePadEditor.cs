using System;
using System.Collections;
using System.Collections.Generic;
using GameLogic.Guide;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace UnityFramework.Editor
{
    [CustomEditor(typeof(UIBeginnerGuideGamePad))]
    public class GuideGamePadEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            base.OnInspectorGUI();
            GUI.enabled = true;
        }
    }
}