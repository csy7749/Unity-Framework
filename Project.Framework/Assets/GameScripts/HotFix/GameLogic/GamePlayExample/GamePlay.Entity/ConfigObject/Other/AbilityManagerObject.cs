using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY
using UnityEditor;
#endif

namespace GameLogic.Combat
{
    public class EffectDescription
    {
        public int Id;
        public string Name;
        public string Description;
    }

    public class AbilityManagerObject
#if !NOT_UNITY
        : SerializedScriptableObject
#endif
    {
#if UNITY_EDITOR
        private static AbilityManagerObject _instance;
        public static AbilityManagerObject Instance
        {
            get
            {
                _instance = AssetDatabase.LoadAssetAtPath<AbilityManagerObject>("Assets/AssetRaw/DefaultPackage/Ability/AbilityManager.asset");
                if (_instance == null)
                {
                    _instance = new AbilityManagerObject();
                    AssetDatabase.CreateAsset(_instance, "Assets/AssetRaw/DefaultPackage/AbilityAbilityManager.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                return _instance;
            }
        }
#endif

        //public string ObjectAssetFolder = "Assets/Resources";

        public string SkillAssetFolder = "Assets/AssetRaw/DefaultPackage/Ability/Skill";
        public string BuffAssetFolder = "Assets/AssetRaw/DefaultPackage/Ability/Buff";
        public string ExecutionAssetFolder = "Assets/AssetRaw/DefaultPackage/Ability/ExecutionObjects";

        //public string SkillExecutionAssetFolder = "Assets/Resources/ExecutionObjects";
        //public string StatusExecutionAssetFolder = "Assets/Resources/ExecutionObjects";

        public const string SkillResFolder = "Ability/Skill";
        public const string BuffResFolder = "Ability/Buff";
        public const string ExecutionResFolder = "ExecutionObjects";

        [Space(10)]
        public Dictionary<int, string> EffectClasses = new Dictionary<int, string>();
        public Dictionary<int, EffectDescription> EffectTypes = new Dictionary<int, EffectDescription>();
    }
}