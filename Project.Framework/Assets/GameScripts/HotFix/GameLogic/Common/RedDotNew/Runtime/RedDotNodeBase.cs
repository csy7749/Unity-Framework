using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLogic.RedDotNew
{
    public class RedDotNodeBase
    {
        protected readonly RedDotNodeBase _parent;
        protected Dictionary<string, RedDotNodeBase> _children;
        protected Action<int> _changeCallbacks;

        public string NodeName { get; }

        public RedDotNodeBase(string nodeName, RedDotNodeBase parent = null)
        {
            NodeName = nodeName;
            _parent = parent;
        }

        public RedDotNodeBase InitNode(string path, bool isView, bool bindRole)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            SplitPath(path, out var currentName, out var remainingPath);
            _children ??= new Dictionary<string, RedDotNodeBase>();

            if (!_children.TryGetValue(currentName, out var child))
            {
                child = CreateNode(currentName, remainingPath, isView, bindRole);
                _children.Add(currentName, child);
            }

            return string.IsNullOrEmpty(remainingPath) ? child : child.InitNode(remainingPath, isView, bindRole);
        }

        public RedDotNodeBase GetRedDotNode(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || _children == null)
            {
                return null;
            }

            SplitPath(path, out var currentName, out var remainingPath);
            if (!_children.TryGetValue(currentName, out var child))
            {
                return null;
            }

            return string.IsNullOrEmpty(remainingPath) ? child : child.GetRedDotNode(remainingPath);
        }

        public virtual void SetStatus(int count)
        {
        }

        public virtual void CalculateCount()
        {
        }

        public virtual void Register(Action<int> callback)
        {
            _changeCallbacks += callback;
        }

        public void Unregister(Action<int> callback)
        {
            _changeCallbacks -= callback;
        }

        public virtual void Clear()
        {
            ResetStatus();
            _changeCallbacks = null;

            if (_children != null)
            {
                var childNames = _children.Keys.ToArray();
                for (var i = 0; i < childNames.Length; i++)
                {
                    _children[childNames[i]].Clear();
                }

                _children.Clear();
            }

            _parent?.RemoveChild(NodeName);
        }

        protected virtual void ResetStatus()
        {
        }

        protected string GetFullPath()
        {
            return _parent == null ? NodeName : $"{_parent.GetFullPath()}/{NodeName}";
        }

        private static void SplitPath(string path, out string currentName, out string remainingPath)
        {
            var separatorIndex = path.IndexOf('/');
            if (separatorIndex < 0)
            {
                currentName = path;
                remainingPath = string.Empty;
                return;
            }

            currentName = path.Substring(0, separatorIndex);
            remainingPath = path.Substring(separatorIndex + 1);
        }

        private RedDotNodeBase CreateNode(string nodeName, string remainingPath, bool isView, bool bindRole)
        {
            if (!string.IsNullOrEmpty(remainingPath))
            {
                return new RedDotNumberNode(nodeName, this);
            }

            return isView ? new RedDotViewNode(nodeName, bindRole, this) : new RedDotNumberNode(nodeName, this);
        }

        private void RemoveChild(string nodeName)
        {
            if (_children == null)
            {
                return;
            }

            _children.Remove(nodeName);
        }
    }
}
