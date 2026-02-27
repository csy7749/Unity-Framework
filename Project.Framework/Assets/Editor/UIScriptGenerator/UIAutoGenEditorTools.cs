using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GameLogic;
using UnityEngine;

namespace UnityFramework.Editor.UI
{
    internal enum NamingConvention
    {
        CamelCase,
        PascalCase,
        SnakeCase,
    }

    internal static class UIAutoGenEditorTools
    {
        public static T FindComponentInParent<T>(Transform child, bool includeSelf = true) where T : UnityEngine.Component
        {
            var start = includeSelf ? child : child.parent;
            return start == null ? null : start.GetComponentInParent<T>(true);
        }

        public static List<T> GetAllComponentsInPrefab<T>(GameObject prefab) where T : UnityEngine.Component
        {
            var components = new List<T>();
            if (prefab == null)
            {
                return components;
            }

            var stack = new Stack<Transform>();
            stack.Push(prefab.transform);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                components.AddRange(current.GetComponents<T>());
                foreach (Transform child in current)
                {
                    stack.Push(child);
                }
            }

            return components;
        }

        public static string GetRelativePath(Transform current, Transform parent, bool includeParent = true)
        {
            if (current == null || parent == null)
            {
                return string.Empty;
            }

            if (current == parent)
            {
                return includeParent ? parent.name : string.Empty;
            }

            var segments = new Stack<string>();
            var node = current;
            while (node != null && node != parent)
            {
                segments.Push(node.name);
                node = node.parent;
            }

            if (node == null)
            {
                return string.Empty;
            }

            if (includeParent)
            {
                segments.Push(parent.name);
            }

            return string.Join("/", segments);
        }

        public static string GetVariableName(string input, NamingConvention convention)
        {
            var words = Regex.Matches(input ?? string.Empty, @"([A-Z][a-z]*|[a-z]+|\d+)")
                .Cast<Match>()
                .Select(m => Regex.Replace(m.Value, @"[^A-Za-z0-9]+", string.Empty))
                .Where(v => !string.IsNullOrEmpty(v))
                .ToArray();

            var result = BuildNameByConvention(words, convention);
            return string.IsNullOrEmpty(result) ? "Node" : EnsureIdentifier(result);
        }

        private static string BuildNameByConvention(string[] words, NamingConvention convention)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < words.Length; i++)
            {
                var word = words[i];
                switch (convention)
                {
                    case NamingConvention.CamelCase:
                        builder.Append(i == 0 ? word.ToLowerInvariant() : ToPascalWord(word));
                        break;
                    case NamingConvention.PascalCase:
                        builder.Append(ToPascalWord(word));
                        break;
                    case NamingConvention.SnakeCase:
                        if (i > 0)
                        {
                            builder.Append("_");
                        }
                        builder.Append(word.ToLowerInvariant());
                        break;
                }
            }

            return builder.ToString();
        }

        private static string ToPascalWord(string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                return string.Empty;
            }

            if (word.Length == 1)
            {
                return char.ToUpperInvariant(word[0]).ToString();
            }

            return char.ToUpperInvariant(word[0]) + word.Substring(1).ToLowerInvariant();
        }

        private static string EnsureIdentifier(string input)
        {
            if (char.IsDigit(input[0]))
            {
                return "_" + input;
            }

            return input;
        }

        public static void RefreshHierarchy(UIControlData rootUIControlData)
        {
            if (rootUIControlData == null)
            {
                return;
            }

            var allControls = rootUIControlData.GetComponentsInChildren<UIControlData>(true);
            var allCtrlItems = CollectAndClearControlItems(allControls);
            RebuildSubRelations(allControls);
            ReattachControlItems(allCtrlItems);
        }

        private static List<CtrlItemData> CollectAndClearControlItems(UIControlData[] allControls)
        {
            var allCtrlItems = new List<CtrlItemData>();
            foreach (var control in allControls)
            {
                allCtrlItems.AddRange(control.CtrlItemDatas);
                control.CtrlItemDatas.Clear();
                control.SubUIItemDatas.Clear();
            }

            return allCtrlItems;
        }

        private static void RebuildSubRelations(UIControlData[] allControls)
        {
            foreach (var control in allControls)
            {
                var parent = FindComponentInParent<UIControlData>(control.transform, false);
                if (parent == null)
                {
                    continue;
                }

                FillDefaultNames(control, parent);
                if (control.GenerateType == UIAutoGenerateType.None || control.GenerateType == UIAutoGenerateType.LoopSubItem)
                {
                    continue;
                }

                parent.AddSubControlData(new SubUIItemData { subUIData = control });
            }
        }

        private static void FillDefaultNames(UIControlData control, UIControlData parent)
        {
            if (string.IsNullOrWhiteSpace(control.VariableName))
            {
                control.VariableName = GetVariableName(control.name, NamingConvention.PascalCase);
            }

            if (string.IsNullOrWhiteSpace(control.ClassName))
            {
                var classSeed = $"{parent.ClassName}_{control.name}";
                control.ClassName = GetVariableName(classSeed, NamingConvention.PascalCase);
            }
        }

        private static void ReattachControlItems(List<CtrlItemData> allCtrlItems)
        {
            foreach (var ctrl in allCtrlItems)
            {
                var target = ctrl.targets is { Length: > 0 } ? ctrl.targets[0] : null;
                var targetTransform = ResolveTransform(target, ctrl.type);
                if (targetTransform == null)
                {
                    continue;
                }

                var parent = FindComponentInParent<UIControlData>(targetTransform);
                if (parent != null)
                {
                    parent.AddControlData(ctrl);
                }
            }
        }

        private static Transform ResolveTransform(UnityEngine.Object target, string typeName)
        {
            if (target == null)
            {
                return null;
            }

            if (typeName == nameof(GameObject))
            {
                return (target as GameObject)?.transform;
            }

            return (target as UnityEngine.Component)?.transform;
        }
    }
}
