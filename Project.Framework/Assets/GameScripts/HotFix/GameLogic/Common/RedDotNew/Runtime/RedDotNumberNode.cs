using System;
using UnityEngine;

namespace GameLogic.RedDotNew
{
    public sealed class RedDotNumberNode : RedDotNodeBase
    {
        public int RedDotCount { get; private set; }

        public RedDotNumberNode(string nodeName, RedDotNodeBase parent = null) : base(nodeName, parent)
        {
        }

        public override void SetStatus(int count)
        {
            if (HasChildren())
            {
                Debug.LogError("非叶子数量红点节点不能直接设置数量。");
                return;
            }

            var target = Math.Max(0, count);
            if (target == RedDotCount)
            {
                return;
            }

            RedDotCount = target;
            CalculateCount();
        }

        public void SetStateByAccumulation(int count)
        {
            if (HasChildren())
            {
                Debug.LogError("非叶子数量红点节点不能直接设置数量。");
                return;
            }

            if (count == 0)
            {
                return;
            }

            var target = Math.Max(0, RedDotCount + count);
            if (target == RedDotCount)
            {
                return;
            }

            RedDotCount = target;
            CalculateCount();
        }

        public override void CalculateCount()
        {
            if (!HasChildren())
            {
                _changeCallbacks?.Invoke(RedDotCount);
                _parent?.CalculateCount();
                return;
            }

            var total = CalculateChildrenCount();
            if (total == RedDotCount)
            {
                return;
            }

            RedDotCount = total;
            _changeCallbacks?.Invoke(RedDotCount);
            _parent?.CalculateCount();
        }

        public override void Register(Action<int> callback)
        {
            base.Register(callback);
            callback?.Invoke(RedDotCount);
        }

        protected override void ResetStatus()
        {
            if (RedDotCount == 0)
            {
                return;
            }

            RedDotCount = 0;
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
