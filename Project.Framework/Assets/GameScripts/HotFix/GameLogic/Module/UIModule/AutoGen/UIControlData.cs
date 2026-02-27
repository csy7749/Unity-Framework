using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    public enum UIAutoGenerateType
    {
        None = 0,
        Window = 1,
        SubItem = 2,
        LoopSubItem = 3,
    }

    [Serializable]
    public sealed class CtrlItemData : IEquatable<CtrlItemData>
    {
        public string name = string.Empty;
        public string type = string.Empty;
        public string parentClassName = string.Empty;
        public UnityEngine.Object[] targets = new UnityEngine.Object[1];

        public bool Equals(CtrlItemData other)
        {
            if (other == null)
            {
                return false;
            }

            var thisTarget = targets is { Length: > 0 } ? targets[0] : null;
            var otherTarget = other.targets is { Length: > 0 } ? other.targets[0] : null;
            return type == other.type && thisTarget == otherTarget;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CtrlItemData);
        }

        public override int GetHashCode()
        {
            var target = targets is { Length: > 0 } ? targets[0] : null;
            return HashCode.Combine(type, target);
        }
    }

    [Serializable]
    public sealed class SubUIItemData : IEquatable<SubUIItemData>
    {
        public UIControlData subUIData;
        public string name => subUIData != null ? subUIData.VariableName : string.Empty;
        public string typeName => subUIData != null ? subUIData.ClassName : string.Empty;

        public bool Equals(SubUIItemData other)
        {
            if (other == null)
            {
                return false;
            }

            return subUIData == other.subUIData && typeName == other.typeName && name == other.name;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SubUIItemData);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(subUIData, typeName, name);
        }
    }

    [Serializable]
    public sealed class UIControlDataPayload
    {
        public UIAutoGenerateType generateType;
        public UILayer uiLayer;
        public bool isAutoCreateCtrl;
        public bool isJustOne;
        public string className;
        public string variableName;
        public string parentClassName;
        public readonly List<CtrlItemData> ctrlItemDatas = new List<CtrlItemData>();
        public readonly List<SubUIItemData> subUIItemDatas = new List<SubUIItemData>();
    }

    [DisallowMultipleComponent]
    public class UIControlData : MonoBehaviour
    {
        public UIAutoGenerateType GenerateType = UIAutoGenerateType.None;
        public UILayer UILayer = UILayer.UI;
        public bool IsAutoCreateCtrl;
        public bool IsJustOne = true;
        public string ClassName = string.Empty;
        public string VariableName = string.Empty;
        public string ParentClassName = string.Empty;

        [SerializeField] private List<CtrlItemData> _ctrlItemDatas = new List<CtrlItemData>();
        [SerializeField] private List<SubUIItemData> _subUIItemDatas = new List<SubUIItemData>();

        public List<CtrlItemData> CtrlItemDatas => _ctrlItemDatas ??= new List<CtrlItemData>();
        public List<SubUIItemData> SubUIItemDatas => _subUIItemDatas ??= new List<SubUIItemData>();

        public void AddControlData(CtrlItemData data)
        {
            if (data == null || CtrlItemDatas.Contains(data))
            {
                return;
            }

            data.parentClassName = ClassName;
            CtrlItemDatas.Add(data);
        }

        public void RemoveControlData(CtrlItemData data)
        {
            if (data == null)
            {
                return;
            }

            CtrlItemDatas.Remove(data);
        }

        public bool IsAdded(CtrlItemData ctrlItemData)
        {
            return ctrlItemData != null && CtrlItemDatas.Contains(ctrlItemData);
        }

        public CtrlItemData GetAdded(CtrlItemData ctrlItemData)
        {
            if (ctrlItemData == null)
            {
                return null;
            }

            foreach (var item in CtrlItemDatas)
            {
                if (item.Equals(ctrlItemData))
                {
                    return item;
                }
            }

            return null;
        }

        public void UpdateUIControlData(CtrlItemData ctrlItemData)
        {
            if (ctrlItemData == null)
            {
                return;
            }

            foreach (var item in CtrlItemDatas)
            {
                if (item.targets is not { Length: > 0 } || item.targets[0] != null || !item.Equals(ctrlItemData))
                {
                    continue;
                }

                item.targets[0] = ctrlItemData.targets[0];
            }
        }

        public void AddSubControlData(SubUIItemData data)
        {
            if (data == null || data.subUIData == null || SubUIItemDatas.Contains(data))
            {
                return;
            }

            data.subUIData.ParentClassName = ClassName;
            SubUIItemDatas.Add(data);
        }

        public void RemoveSubControlData(SubUIItemData data)
        {
            if (data == null)
            {
                return;
            }

            SubUIItemDatas.Remove(data);
        }

        public void ReplaceSubUIControlData(UIControlData oldData, UIControlData newData)
        {
            if (oldData == null || newData == null)
            {
                return;
            }

            foreach (var item in SubUIItemDatas)
            {
                if (item.subUIData == oldData)
                {
                    item.subUIData = newData;
                }
            }
        }

        public UIControlDataPayload CapturePayload()
        {
            var payload = new UIControlDataPayload
            {
                generateType = GenerateType,
                uiLayer = UILayer,
                isAutoCreateCtrl = IsAutoCreateCtrl,
                isJustOne = IsJustOne,
                className = ClassName,
                variableName = VariableName,
                parentClassName = ParentClassName,
            };

            CopyControlItems(CtrlItemDatas, payload.ctrlItemDatas);
            CopySubItems(SubUIItemDatas, payload.subUIItemDatas);
            return payload;
        }

        public void ApplyPayload(UIControlDataPayload payload)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            GenerateType = payload.generateType;
            UILayer = payload.uiLayer;
            IsAutoCreateCtrl = payload.isAutoCreateCtrl;
            IsJustOne = payload.isJustOne;
            ClassName = payload.className;
            VariableName = payload.variableName;
            ParentClassName = payload.parentClassName;

            CtrlItemDatas.Clear();
            SubUIItemDatas.Clear();
            CopyControlItems(payload.ctrlItemDatas, CtrlItemDatas);
            CopySubItems(payload.subUIItemDatas, SubUIItemDatas);
        }

        private static void CopyControlItems(List<CtrlItemData> source, List<CtrlItemData> target)
        {
            if (source == null)
            {
                return;
            }

            foreach (var item in source)
            {
                if (item == null)
                {
                    continue;
                }

                var copy = new CtrlItemData
                {
                    name = item.name,
                    type = item.type,
                    parentClassName = item.parentClassName,
                    targets = (UnityEngine.Object[])item.targets.Clone(),
                };
                target.Add(copy);
            }
        }

        private static void CopySubItems(List<SubUIItemData> source, List<SubUIItemData> target)
        {
            if (source == null)
            {
                return;
            }

            foreach (var item in source)
            {
                if (item?.subUIData == null)
                {
                    continue;
                }

                target.Add(new SubUIItemData
                {
                    subUIData = item.subUIData,
                });
            }
        }
    }
}
