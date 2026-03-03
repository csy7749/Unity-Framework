using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic.RedDotNew
{
    [CreateAssetMenu(fileName = "RedDotConfigAsset", menuName = "GameLogic/RedDotConfigAsset")]
    public sealed class RedDotConfigAsset : ScriptableObject
    {
        private static RedDotConfigAsset _instance;

        public static RedDotConfigAsset Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<RedDotConfigAsset>("RedDotConfigAsset");
                    _instance?.Init();
                }

                return _instance;
            }
        }

        public List<RedDotConfigData> Data = new List<RedDotConfigData>();

        [NonSerialized] public Dictionary<int, RedDotConfigData> DataDic = new Dictionary<int, RedDotConfigData>();

        public void Init()
        {
            Data ??= new List<RedDotConfigData>();
            DataDic = new Dictionary<int, RedDotConfigData>(Data.Count);
            ResetIsChildData();

            for (var i = 0; i < Data.Count; i++)
            {
                var item = Data[i];
                if (item == null)
                {
                    continue;
                }

                if (!DataDic.TryAdd(item.Id, item))
                {
                    Debug.LogWarning($"RedDotConfigAsset duplicate id: {item.Id}");
                }
            }
        }

        private void OnEnable()
        {
            Init();
        }

        private void ResetIsChildData()
        {
            for (var i = 0; i < Data.Count; i++)
            {
                var current = Data[i];
                if (current == null || string.IsNullOrWhiteSpace(current.Path))
                {
                    continue;
                }

                current.IsChild = true;
                var currentPrefix = current.Path + "/";
                for (var j = 0; j < Data.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    var other = Data[j];
                    if (other == null || string.IsNullOrWhiteSpace(other.Path))
                    {
                        continue;
                    }

                    if (other.Path.StartsWith(currentPrefix, StringComparison.Ordinal))
                    {
                        current.IsChild = false;
                        break;
                    }
                }
            }
        }

        [Serializable]
        public sealed class RedDotConfigData
        {
            public int Id;
            public string Path;
            public bool IsView;
            public ViewType ViewType = ViewType.Once;
            public RedDotShowType ShowType = RedDotShowType.Normal;
            public bool IsChild = true;
            public bool BindRole = true;
            public string Alias;
            public bool UseLocalSave;
        }
    }
}
