using System;
using System.Globalization;
using UnityEngine;

namespace GameLogic.RedDotNew
{
    public sealed class RedDotTree
    {
        private const string TreeRootName = "RedDotTreeRoot";
        private static RedDotTree _instance;

        public static RedDotTree Instance => _instance ??= CreateInstance();

        public static string UniqueKey = string.Empty;

        public RedDotNodeBase Root { get; }

        private RedDotTree()
        {
            Root = new RedDotNumberNode(TreeRootName);
        }

        public RedDotNodeBase InitRedDotNode(string path, bool isView, bool bindRole, ViewType viewType, bool useLocalSave)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            var node = Root.InitNode(path, isView, bindRole);
            if (node == null)
            {
                return null;
            }

            InitializeStatus(path, node, isView, bindRole, viewType, useLocalSave);
            return node;
        }

        public void ChangeRedDotCount(int redDotId, int count, params object[] args)
        {
            if (!TryGetConfig(redDotId, out var config))
            {
                return;
            }

            var path = FormatPath(config.Path, args);
            var node = GetOrCreateNode(path, config);
            if (node is not RedDotNumberNode numberNode)
            {
                Debug.LogWarning($"红点类型不匹配，路径 {path} 不是数量型节点。");
                return;
            }

            numberNode.SetStatus(count);
            SaveNumberNodeIfNeeded(config, path, numberNode.RedDotCount);
        }

        public void ChangeRedDotCountByAccumulation(int redDotId, int count, params object[] args)
        {
            if (!TryGetConfig(redDotId, out var config))
            {
                return;
            }

            var path = FormatPath(config.Path, args);
            var node = GetOrCreateNode(path, config);
            if (node is not RedDotNumberNode numberNode)
            {
                Debug.LogWarning($"红点类型不匹配，路径 {path} 不是数量型节点。");
                return;
            }

            numberNode.SetStateByAccumulation(count);
            SaveNumberNodeIfNeeded(config, path, numberNode.RedDotCount);
        }

        public void SetWaitWatch(int redDotId, params object[] args)
        {
            if (!TryGetConfig(redDotId, out var config))
            {
                return;
            }

            var path = FormatPath(config.Path, args);
            var node = GetOrCreateNode(path, config);
            if (node is not RedDotViewNode viewNode)
            {
                Debug.LogWarning($"红点类型不匹配，路径 {path} 不是查看型节点。");
                return;
            }

            var shouldWaitWatch = ShouldSetWaitWatch(config.ViewType, GetLocalSaveData(config.BindRole, BuildTreePath(path)));
            viewNode.SetStatus(shouldWaitWatch ? 1 : 0);
        }

        public void Watch(int redDotId, params object[] args)
        {
            if (!TryGetConfig(redDotId, out var config))
            {
                return;
            }

            var path = FormatPath(config.Path, args);
            var node = GetOrCreateNode(path, config);
            node?.SetStatus(0);
        }

        public void Watch(string path)
        {
            GetRedDotNode(path)?.SetStatus(0);
        }

        public void Register(int redDotId, Action<int> changeCallback, params object[] args)
        {
            if (changeCallback == null || !TryGetConfig(redDotId, out var config))
            {
                return;
            }

            var path = FormatPath(config.Path, args);
            var node = GetOrCreateNode(path, config);
            node?.Register(changeCallback);
        }

        public void Unregister(string path, Action<int> changeCallback)
        {
            GetRedDotNode(path)?.Unregister(changeCallback);
        }

        public void RemoveRedDotNode(int redDotId, params object[] args)
        {
            if (!TryGetConfig(redDotId, out var config))
            {
                return;
            }

            var path = FormatPath(config.Path, args);
            var node = GetRedDotNode(path);
            if (node == null)
            {
                Debug.LogWarning($"移除红点节点失败，路径不存在：{path}");
                return;
            }

            node.Clear();
        }

        public static void LocalSave(bool bindRole, string key, string value)
        {
            PlayerPrefs.SetString(BuildLocalKey(bindRole, key), value);
        }

        public static void RemoveLocalSave(bool bindRole, string key)
        {
            var localKey = BuildLocalKey(bindRole, key);
            if (PlayerPrefs.HasKey(localKey))
            {
                PlayerPrefs.DeleteKey(localKey);
            }
        }

        public static string GetLocalSaveData(bool bindRole, string key)
        {
            return PlayerPrefs.GetString(BuildLocalKey(bindRole, key));
        }

        private static RedDotTree CreateInstance()
        {
            var tree = new RedDotTree();
            tree.PreInitializeNodes();
            return tree;
        }

        private void PreInitializeNodes()
        {
            var configAsset = RedDotConfigAsset.Instance;
            if (configAsset?.Data == null)
            {
                return;
            }

            for (var i = 0; i < configAsset.Data.Count; i++)
            {
                var config = configAsset.Data[i];
                if (config == null || string.IsNullOrWhiteSpace(config.Path))
                {
                    continue;
                }

                if (ContainsFormatPlaceholder(config.Path))
                {
                    continue;
                }

                InitRedDotNode(config.Path, config.IsView, config.BindRole, config.ViewType, config.UseLocalSave);
            }
        }

        private void InitializeStatus(
            string path,
            RedDotNodeBase node,
            bool isView,
            bool bindRole,
            ViewType viewType,
            bool useLocalSave)
        {
            if (isView)
            {
                var timestamp = GetLocalSaveData(bindRole, BuildTreePath(path));
                node.SetStatus(ShouldSetWaitWatch(viewType, timestamp) ? 1 : 0);
                return;
            }

            if (!useLocalSave || node is not RedDotNumberNode)
            {
                return;
            }

            var countText = GetLocalSaveData(bindRole, path);
            if (int.TryParse(countText, out var count))
            {
                node.SetStatus(count);
            }
        }

        private bool TryGetConfig(int redDotId, out RedDotConfigAsset.RedDotConfigData config)
        {
            config = null;
            var configAsset = RedDotConfigAsset.Instance;
            if (configAsset?.DataDic == null)
            {
                return false;
            }

            return configAsset.DataDic.TryGetValue(redDotId, out config);
        }

        private RedDotNodeBase GetOrCreateNode(string path, RedDotConfigAsset.RedDotConfigData config)
        {
            var node = GetRedDotNode(path);
            return node ?? InitRedDotNode(path, config.IsView, config.BindRole, config.ViewType, config.UseLocalSave);
        }

        private RedDotNodeBase GetRedDotNode(string path)
        {
            return Root.GetRedDotNode(path);
        }

        private static string FormatPath(string pathTemplate, object[] args)
        {
            if (args == null || args.Length == 0)
            {
                return pathTemplate;
            }

            return string.Format(pathTemplate, args);
        }

        private static bool ContainsFormatPlaceholder(string path)
        {
            return path.Contains("{") || path.Contains("}");
        }

        private void SaveNumberNodeIfNeeded(RedDotConfigAsset.RedDotConfigData config, string path, int count)
        {
            if (!config.UseLocalSave)
            {
                return;
            }

            LocalSave(config.BindRole, path, count.ToString());
        }

        private static string BuildTreePath(string path)
        {
            return $"{TreeRootName}/{path}";
        }

        private static string BuildLocalKey(bool bindRole, string key)
        {
            return bindRole ? UniqueKey + key : key;
        }

        private bool ShouldSetWaitWatch(ViewType viewType, string timestampText)
        {
            if (string.IsNullOrWhiteSpace(timestampText))
            {
                return true;
            }

            if (!long.TryParse(timestampText, out var unixSeconds))
            {
                return true;
            }

            var lastView = DateTimeOffset.FromUnixTimeSeconds(unixSeconds).UtcDateTime;
            var now = DateTime.UtcNow;

            switch (viewType)
            {
                case ViewType.Once:
                    return false;
                case ViewType.Day:
                    return lastView.Date != now.Date;
                case ViewType.Week:
                    return !IsInSameWeek(lastView, now);
                case ViewType.Month:
                    return lastView.Year != now.Year || lastView.Month != now.Month;
                default:
                    return true;
            }
        }

        private bool IsInSameWeek(DateTime dateTimeA, DateTime dateTimeB)
        {
            var calendar = CultureInfo.InvariantCulture.Calendar;
            var weekA = calendar.GetWeekOfYear(dateTimeA, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            var weekB = calendar.GetWeekOfYear(dateTimeB, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return dateTimeA.Year == dateTimeB.Year && weekA == weekB;
        }
    }
}
