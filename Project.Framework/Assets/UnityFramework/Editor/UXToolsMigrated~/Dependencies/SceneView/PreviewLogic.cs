#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_2021_2_OR_NEWER
using PrefabStage = UnityEditor.SceneManagement.PrefabStage;
#else
using PrefabStage = UnityEditor.Experimental.SceneManagement.PrefabStage;
#endif

namespace UnityFramework.Editor
{
    [UXInitialize]
    public class PreviewLogic
    {
        private const string PreviewFlagKey = "InPreview";
        private const string PreviewCanvasName = "UXPreviewCanvas";

        static PreviewLogic()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        public static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (EditorPrefs.GetBool(PreviewFlagKey) && state == PlayModeStateChange.EnteredEditMode)
            {
                ExitPreview();
            }
        }

        public static void Preview()
        {
            EditorPrefs.SetBool(PreviewFlagKey, true);
            PrefabStage prefabStage = PrefabStageUtils.GetCurrentPrefabStage();
            if (prefabStage == null)
            {
                EditorUtility.DisplayDialog(
                    "messageBox",
                    "请打开Prefab后再进行预览",
                    "确定",
                    "取消");
                return;
            }

            InitPreviewScene(prefabStage);
            Utils.ExitPrefabStage();
            Utils.EnterPlayMode();
        }

        public static void ExitPreview()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            ResumeOriginScene();
        }

        public static void InitPreviewScene(PrefabStage prefabStage)
        {
            string prefabPath = prefabStage.GetAssetPath();
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            EditorPrefs.SetString(ThunderFireUIToolConfig.PreviewPrefabPath, prefabPath);

            GameObject previewCanvas = GameObject.Find(PreviewCanvasName);
            if (previewCanvas == null)
            {
                Scene originScene = SceneManager.GetActiveScene();
                EditorPrefs.SetString(ThunderFireUIToolConfig.PreviewOriginScene, originScene.path);
                bool shouldSave = EditorUtility.DisplayDialog(
                    "Save",
                    "是否想要保存场景",
                    "确定",
                    "取消");
                if (shouldSave)
                {
                    EditorSceneManager.SaveScene(originScene);
                }

                EditorSceneManager.OpenScene(ThunderFireUIToolConfig.ScenePath + "PreviewScene.unity", OpenSceneMode.Single);
                previewCanvas = GameObject.Find(PreviewCanvasName);
                InitPreviewCanvas(prefab, previewCanvas);
                return;
            }

            RefreshPreviewCanvas(prefab, previewCanvas);
        }

        public static void ResumeOriginScene()
        {
            Utils.StopPlayMode();
            string prefabPath = EditorPrefs.GetString(ThunderFireUIToolConfig.PreviewPrefabPath);
            string originScenePath = EditorPrefs.GetString(ThunderFireUIToolConfig.PreviewOriginScene);

            if (!string.IsNullOrEmpty(originScenePath))
            {
                EditorSceneManager.OpenScene(originScenePath, OpenSceneMode.Single);
                if (!string.IsNullOrEmpty(prefabPath))
                {
                    Utils.OpenPrefab(prefabPath);
                }
            }

            EditorPrefs.DeleteKey(ThunderFireUIToolConfig.PreviewPrefabPath);
            EditorPrefs.DeleteKey(ThunderFireUIToolConfig.PreviewOriginScene);
            EditorPrefs.DeleteKey(PreviewFlagKey);
        }

        private static GameObject InitPreviewCanvas(GameObject prefab, GameObject previewCanvas)
        {
            return Object.Instantiate(prefab, previewCanvas.transform);
        }

        private static GameObject RefreshPreviewCanvas(GameObject prefab, GameObject previewCanvas)
        {
            ClearPreviewCanvas(previewCanvas);
            return InitPreviewCanvas(prefab, previewCanvas);
        }

        private static void ClearPreviewCanvas(GameObject previewCanvas)
        {
            for (int i = previewCanvas.transform.childCount - 1; i > 0; i--)
            {
                Object.DestroyImmediate(previewCanvas.transform.GetChild(i).gameObject);
            }
        }
    }
}
#endif
