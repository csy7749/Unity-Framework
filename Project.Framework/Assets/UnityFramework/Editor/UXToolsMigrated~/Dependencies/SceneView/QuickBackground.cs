#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
#if UNITY_2021_2_OR_NEWER
using PrefabStage = UnityEditor.SceneManagement.PrefabStage;
#else
using PrefabStage = UnityEditor.Experimental.SceneManagement.PrefabStage;
#endif

namespace UnityFramework.Editor
{
    [UXInitialize(100)]
    [Serializable]
    public partial class QuickBackground
    {
        private const string RootName = "UXQuickBackground";
        private const string ImageName = "UXQuickBackgroundImage";
        private const string SceneGuid = "-1";
        private const string RootLabel = "Quick Background";
        private const string ChildLabel = "Reference Background";
        private const float SaveButtonOffset = 50f;
        private const float CloseButtonOffset = 25f;
        private const float ButtonSize = 25f;

        public static bool isOpen;
        public static string name;
        public static string childName;

        private static Vector3 _position = default;
        private static Vector3 _rotation = default;
        private static Vector3 _scale = Vector3.one;
        private static Vector2 _size = new Vector2(1920f, 1080f);
        private static Color _color = Color.white;
        private static Sprite _sprite = default;
        private static bool _inPrefabStage;
        private static bool _expandPending = true;
        private static Texture2D _saveIcon;
        private static Texture2D _closeIcon;

        private static Texture2D SaveIcon => _saveIcon ??= ToolUtils.GetIcon("save") as Texture2D;
        private static Texture2D CloseIcon => _closeIcon ??= ToolUtils.GetIcon("close") as Texture2D;

        static QuickBackground()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorSceneManager.sceneClosing += OnSceneClosing;
            PrefabStageUtils.AddOpenedEvent(OnPrefabStageOpened);
            PrefabStageUtils.AddClosingEvent(OnPrefabStageClosing);
        }

        [MenuItem(ThunderFireUIToolConfig.Menu_BackGround, false, 151)]
        public static void CreateBackGround()
        {
            if (!SwitchSetting.CheckValid(SwitchSetting.SwitchType.QuickBackground))
            {
                return;
            }

            _inPrefabStage = PrefabStageUtils.GetCurrentPrefabStage() != null;
            Load();
            isOpen = true;
            SetBackGround();
        }

        [UnityEditor.Callbacks.DidReloadScripts(0)]
        private static void OnScriptReload()
        {
            if (!SwitchSetting.CheckValid(SwitchSetting.SwitchType.QuickBackground))
            {
                return;
            }

            _inPrefabStage = PrefabStageUtils.GetCurrentPrefabStage() != null;
            name = RootName;
            childName = ImageName;
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
        }

        public static void SetBackGround()
        {
            if (!SwitchSetting.CheckValid(SwitchSetting.SwitchType.QuickBackground) || !isOpen)
            {
                return;
            }

            if (GetCurrentBackgroundObject() != null)
            {
                return;
            }

            GameObject instance = CreateBackgroundInstance();
            Transform imageTransform = GetImageTransform(instance);
            ApplyRuntimeState(imageTransform);
            instance.hideFlags = HideFlags.DontSave;
            imageTransform.gameObject.hideFlags = HideFlags.DontSave;
            SceneVisibilityManager.instance.DisablePicking(instance, true);
            SceneHierarchyUtility.SetExpanded(instance, true);
            ExpandParentIfNeeded(instance.transform);
            name = instance.name;
            childName = ImageName;
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
        }

        public static void Serialize()
        {
            QuickBackgroundData data = EnsureDataAsset();
            GameObject background = GetCurrentBackgroundObject();
            QuickBackgroundDetail detail = GetCurrentDetail(data);
            if (detail == null)
            {
                return;
            }

            if (background == null)
            {
                detail.isOpen = false;
                JsonAssetManager.SaveAssets(data);
                return;
            }

            WriteDetailFromObject(detail, background);
            JsonAssetManager.SaveAssets(data);
        }

        public static void Close()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
            isOpen = false;
            Serialize();
            DestroyBackgroundObject();
        }

        public static void Load()
        {
            _expandPending = true;
            DestroyBackgroundObject();
            QuickBackgroundDetail detail = GetLoadedDetail();
            if (detail == null)
            {
                detail = CreateDefaultDetail();
            }

            isOpen = detail.isOpen;
            _position = detail.position;
            _rotation = detail.rotation;
            _scale = detail.scale;
            _size = detail.size;
            _color = detail.color;
            _sprite = LoadSprite(detail.spriteId);
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (!SwitchSetting.CheckValid(SwitchSetting.SwitchType.QuickBackground))
            {
                return;
            }

            if (state == PlayModeStateChange.EnteredEditMode)
            {
                _inPrefabStage = PrefabStageUtils.GetCurrentPrefabStage() != null;
                Load();
                SetBackGround();
            }
            else if (state == PlayModeStateChange.ExitingEditMode)
            {
                Serialize();
                DestroyBackgroundObject();
            }
        }

        private static void OnSceneClosing(UnityEngine.SceneManagement.Scene _, bool __)
        {
            if (!SwitchSetting.CheckValid(SwitchSetting.SwitchType.QuickBackground))
            {
                return;
            }

            Serialize();
            DestroyBackgroundObject();
        }

        private static void OnPrefabStageOpened(PrefabStage _)
        {
            if (!SwitchSetting.CheckValid(SwitchSetting.SwitchType.QuickBackground))
            {
                return;
            }

            _inPrefabStage = PrefabStageUtils.GetCurrentPrefabStage() != null;
            Load();
            SetBackGround();
        }

        private static void OnPrefabStageClosing(PrefabStage stage)
        {
            if (!SwitchSetting.CheckValid(SwitchSetting.SwitchType.QuickBackground))
            {
                return;
            }

            GameObject background = GetStageBackgroundObject(stage);
            if (background == null)
            {
                return;
            }

            QuickBackgroundData data = EnsureDataAsset();
            QuickBackgroundDetail detail = GetOrCreatePrefabDetail(data, stage);
            WriteDetailFromObject(detail, background);
            JsonAssetManager.SaveAssets(data);
            UnityEngine.Object.DestroyImmediate(background);
        }

        private static void OnHierarchyGUI(int instanceId, Rect selectionRect)
        {
            GameObject go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (go == null)
            {
                return;
            }

            if (go.name == childName)
            {
                Utils.DrawGreenRect(instanceId, selectionRect, ChildLabel);
                DrawButtons(selectionRect);
                return;
            }

            if (go.name != name)
            {
                return;
            }

            TryExpandHierarchy(go);
            Utils.DrawGreenRect(instanceId, selectionRect, RootLabel);
        }

    }
}
#endif
