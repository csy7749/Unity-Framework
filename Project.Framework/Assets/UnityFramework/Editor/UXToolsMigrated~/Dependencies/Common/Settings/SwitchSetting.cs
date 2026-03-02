#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityFramework.Editor
{
    public class SwitchSetting
    {
        [SerializeField]
        private bool[] _values;

        public enum SwitchType
        {
            RecentlyOpened,
            AlignSnap,
            RightClickList,
            QuickCopy,
            MovementShortcuts,
            PrefabMultiOpen,
            ResolutionAdjustment,
            PrefabResourceCheck,
            QuickBackground,
            AutoConvertTex,
            EnableGamePadBeginnerGuide,
            RecentlySelected,
        }

        private static SwitchSetting _instance;

        public static void Create()
        {
            _instance = JsonAssetManager.CreateAssets<SwitchSetting>(ThunderFireUIToolConfig.SwitchSettingPath);
            int count = Enum.GetValues(typeof(SwitchType)).Length;
            _instance._values = new bool[count];

            for (int i = 0; i < count; i++)
            {
                _instance._values[i] = true;
            }

            _instance._values[(int)SwitchType.PrefabResourceCheck] = false;
            _instance._values[(int)SwitchType.AutoConvertTex] = false;
            _instance._values[(int)SwitchType.EnableGamePadBeginnerGuide] = false;
            JsonAssetManager.SaveAssets(_instance);
        }

        public static void ChangeSwitch(Toggle[] toggles)
        {
            _instance = JsonAssetManager.GetAssets<SwitchSetting>() ??
                JsonAssetManager.CreateAssets<SwitchSetting>(ThunderFireUIToolConfig.SwitchSettingPath);

            _instance._values = new bool[toggles.Length];
            for (int i = 0; i < toggles.Length; i++)
            {
                _instance._values[i] = toggles[i].value;
            }

            JsonAssetManager.SaveAssets(_instance);
            if (_instance._values[(int)SwitchType.EnableGamePadBeginnerGuide])
            {
                ScriptingDefineSymbolUtils.EnableInputSystemDefineSymbol();
            }
            else
            {
                ScriptingDefineSymbolUtils.DisableInputSystemDefineSymbol();
            }

            SceneViewToolBar.CloseFunction();
            SceneViewToolBar.InitFunction();
        }

        public static bool CheckValid(int index)
        {
            if (_instance == null)
            {
                _instance = JsonAssetManager.GetAssets<SwitchSetting>();
            }

            if (_instance == null || _instance._values == null || _instance._values.Length <= index)
            {
                return true;
            }

            return _instance._values[index];
        }

        public static bool CheckValid(SwitchType type)
        {
            return CheckValid((int)type);
        }
    }
}
#endif
