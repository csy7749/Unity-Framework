#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace UnityFramework.Editor
{
    public class PrefabTabsData
    {
        [SerializeField]
        private List<string> _tabs;

        public static List<string> Tabs
        {
            get
            {
                PrefabTabsData instance = JsonAssetManager.GetAssets<PrefabTabsData>();
                if (instance == null)
                {
                    instance = JsonAssetManager.CreateAssets<PrefabTabsData>(ThunderFireUIToolConfig.PrefabTabsPath);
                    instance._tabs = new List<string>();
                }

                return instance._tabs ?? new List<string>();
            }
        }

        public static void Create()
        {
            JsonAssetManager.CreateAssets<PrefabTabsData>(ThunderFireUIToolConfig.PrefabTabsPath);
        }

        public static void SyncTab(List<string> list)
        {
            PrefabTabsData instance = JsonAssetManager.GetAssets<PrefabTabsData>();
            if (instance == null)
            {
                instance = JsonAssetManager.CreateAssets<PrefabTabsData>(ThunderFireUIToolConfig.PrefabTabsPath);
            }

            if (instance._tabs == null)
            {
                instance._tabs = new List<string>();
            }

            instance._tabs.Clear();
            foreach (string item in list)
            {
                instance._tabs.Add(item);
            }

            JsonAssetManager.SaveAssets(instance);
        }
    }
}
#endif
