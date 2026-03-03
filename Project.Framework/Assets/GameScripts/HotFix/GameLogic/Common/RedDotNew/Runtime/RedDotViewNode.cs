using System;
using UnityEngine;

namespace GameLogic.RedDotNew
{
    public sealed class RedDotViewNode : RedDotNodeBase
    {
        private readonly bool _bindRole;
        private bool _viewed = true;

        public bool Viewed => _viewed;

        public RedDotViewNode(string nodeName, bool bindRole, RedDotNodeBase parent = null) : base(nodeName, parent)
        {
            _bindRole = bindRole;
        }

        public override void SetStatus(int count)
        {
            var viewed = count <= 0;
            UpdateViewedState(viewed, true);
        }

        public override void CalculateCount()
        {
            if (!HasChildren())
            {
                _changeCallbacks?.Invoke(_viewed ? 0 : 1);
                _parent?.CalculateCount();
                return;
            }

            var total = CalculateChildrenCount();
            var viewed = total <= 0;
            if (viewed == _viewed)
            {
                return;
            }

            _viewed = viewed;
            _changeCallbacks?.Invoke(viewed ? 0 : 1);
            _parent?.CalculateCount();
        }

        public override void Register(Action<int> callback)
        {
            base.Register(callback);
            callback?.Invoke(_viewed ? 0 : 1);
        }

        protected override void ResetStatus()
        {
            var path = GetFullPath();
            RedDotTree.RemoveLocalSave(_bindRole, path);

            if (!_viewed)
            {
                return;
            }

            _viewed = false;
            CalculateCount();
        }

        private void UpdateViewedState(bool viewed, bool persistWhenViewed)
        {
            if (HasChildren())
            {
                Debug.LogWarning("非叶子查看红点节点不能直接设置状态。");
                return;
            }

            if (_viewed == viewed)
            {
                return;
            }

            _viewed = viewed;
            if (persistWhenViewed && viewed)
            {
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                RedDotTree.LocalSave(_bindRole, GetFullPath(), timestamp);
            }

            CalculateCount();
        }

        private int CalculateChildrenCount()
        {
            var total = 0;
            foreach (var child in _children.Values)
            {
                if (child is RedDotNumberNode numberNode)
                {
                    total += numberNode.RedDotCount;
                    continue;
                }

                if (child is RedDotViewNode viewNode)
                {
                    total += viewNode.Viewed ? 0 : 1;
                }
            }

            return total;
        }

        private bool HasChildren()
        {
            return _children != null && _children.Count > 0;
        }
    }
}
