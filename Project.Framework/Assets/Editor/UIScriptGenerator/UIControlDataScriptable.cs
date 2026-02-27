using System;
using System.Collections.Generic;
using GameLogic;
using UnityEngine;

namespace UnityFramework.Editor.UI
{
    [Serializable]
    public sealed class UIControlDataNode
    {
        public string path;
        public UIAutoGenerateType generateType;
        public UILayer uiLayer;
        public bool isAutoCreateCtrl;
        public bool isJustOne;
        public string className;
        public string variableName;
        public string parentClassName;
        public List<UIControlDataCtrlNode> controls = new List<UIControlDataCtrlNode>();
        public List<UIControlDataNode> subUIs = new List<UIControlDataNode>();
    }

    [Serializable]
    public sealed class UIControlDataCtrlNode
    {
        public string name;
        public string type;
        public string parentClassName;
        public string targetPath;
    }

    [CreateAssetMenu(fileName = "UIControlDataDatabase", menuName = "UnityFramework/UIControlDataDatabase")]
    public sealed class UIControlDataScriptable : ScriptableObject
    {
        public GameObject ownerPrefab;
        public UIControlDataNode rootNode;
    }
}
