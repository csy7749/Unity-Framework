#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_2021_2_OR_NEWER
using PrefabStage = UnityEditor.SceneManagement.PrefabStage;
#else
using PrefabStage = UnityEditor.Experimental.SceneManagement.PrefabStage;
#endif

namespace UnityFramework.Editor
{
    public partial class QuickBackground
    {
        private static void DrawButtons(Rect selectionRect)
        {
            Rect saveRect = new Rect(selectionRect.x + selectionRect.width - SaveButtonOffset, selectionRect.y + selectionRect.height - ButtonSize, ButtonSize, ButtonSize);
            if (GUI.Button(saveRect, SaveIcon))
            {
                Serialize();
            }

            Rect closeRect = new Rect(selectionRect.x + selectionRect.width - CloseButtonOffset, selectionRect.y + selectionRect.height - ButtonSize, ButtonSize, ButtonSize);
            if (GUI.Button(closeRect, CloseIcon))
            {
                Close();
            }
        }

        private static void TryExpandHierarchy(GameObject root)
        {
            if (!_expandPending)
            {
                return;
            }

            _expandPending = false;
            ExpandParentIfNeeded(root.transform);
            SceneHierarchyUtility.SetExpanded(root, true);
            ExpandParentIfNeeded(root.transform);
        }

        private static void ExpandParentIfNeeded(Transform root)
        {
            if (!_inPrefabStage || root.parent == null)
            {
                return;
            }

            SceneHierarchyUtility.SetExpanded(root.parent.gameObject, true);
        }

        private static GameObject CreateBackgroundInstance()
        {
            string prefabPath = $"{ThunderFireUIToolConfig.UXToolsPath}Assets/Editor/Res/UXQuickBackground.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                throw new InvalidOperationException($"Missing quick background prefab: {prefabPath}");
            }

            GameObject instance = UnityEngine.Object.Instantiate(prefab);
            instance.name = RootName;
            instance.transform.SetSiblingIndex(0);
            instance.transform.localScale = Vector3.one;

            if (!_inPrefabStage)
            {
                return instance;
            }

            PrefabStage stage = PrefabStageUtils.GetCurrentPrefabStage();
            if (stage == null)
            {
                throw new InvalidOperationException("Prefab stage was expected but is unavailable.");
            }

            instance.transform.SetParent(stage.prefabContentsRoot.transform, false);
            return instance;
        }

        private static void ApplyRuntimeState(Transform imageTransform)
        {
            imageTransform.localPosition = _position;
            imageTransform.eulerAngles = _rotation;
            imageTransform.localScale = _scale;
            RectTransform rectTransform = imageTransform.GetComponent<RectTransform>();
            Image image = imageTransform.GetComponent<Image>();
            if (rectTransform == null || image == null)
            {
                throw new InvalidOperationException("UXQuickBackgroundImage is missing RectTransform or Image component.");
            }

            rectTransform.sizeDelta = _size;
            image.color = _color;
            image.sprite = _sprite;
        }

        private static Transform GetImageTransform(GameObject root)
        {
            if (root.transform.childCount == 0)
            {
                throw new InvalidOperationException("UXQuickBackground prefab must contain one child image node.");
            }

            return root.transform.GetChild(0);
        }

        private static GameObject GetCurrentBackgroundObject()
        {
            if (!_inPrefabStage)
            {
                return GameObject.Find($"/{RootName}");
            }

            PrefabStage stage = PrefabStageUtils.GetCurrentPrefabStage();
            return GetStageBackgroundObject(stage);
        }

        private static GameObject GetStageBackgroundObject(PrefabStage stage)
        {
            return stage?.prefabContentsRoot?.transform.Find(RootName)?.gameObject;
        }

        private static void DestroyBackgroundObject()
        {
            GameObject background = GetCurrentBackgroundObject();
            if (background != null)
            {
                UnityEngine.Object.DestroyImmediate(background);
            }
        }

        private static QuickBackgroundData EnsureDataAsset()
        {
            QuickBackgroundData data = JsonAssetManager.GetAssets<QuickBackgroundData>() ?? JsonAssetManager.CreateAssets<QuickBackgroundData>(ThunderFireUIToolConfig.QuickBackgroundDataPath);
            data.list ??= new List<QuickBackgroundDataSingle>();
            return data;
        }

        private static QuickBackgroundDetail GetLoadedDetail()
        {
            QuickBackgroundData data = EnsureDataAsset();
            if (!_inPrefabStage)
            {
                return GetOrCreateSceneDetail(data);
            }

            PrefabStage stage = PrefabStageUtils.GetCurrentPrefabStage();
            if (stage == null)
            {
                return null;
            }

            string guid = AssetDatabase.AssetPathToGUID(GetStageAssetPath(stage));
            return FindDetail(data.list, guid);
        }

        private static QuickBackgroundDetail GetCurrentDetail(QuickBackgroundData data)
        {
            if (!_inPrefabStage)
            {
                return GetOrCreateSceneDetail(data);
            }

            PrefabStage stage = PrefabStageUtils.GetCurrentPrefabStage();
            if (stage == null)
            {
                return null;
            }

            return GetOrCreatePrefabDetail(data, stage);
        }

        private static QuickBackgroundDetail GetOrCreateSceneDetail(QuickBackgroundData data)
        {
            return GetOrCreateDetail(data, SceneGuid, SceneGuid);
        }

        private static QuickBackgroundDetail GetOrCreatePrefabDetail(QuickBackgroundData data, PrefabStage stage)
        {
            string assetPath = GetStageAssetPath(stage);
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            return GetOrCreateDetail(data, guid, fileName);
        }

        private static QuickBackgroundDetail GetOrCreateDetail(QuickBackgroundData data, string guid, string displayName)
        {
            QuickBackgroundDataSingle item = data.list.Find(v => v.guid == guid);
            if (item == null)
            {
                item = new QuickBackgroundDataSingle
                {
                    name = displayName,
                    guid = guid,
                    detail = CreateDefaultDetail()
                };
                data.list.Add(item);
                JsonAssetManager.SaveAssets(data);
            }

            if (string.IsNullOrEmpty(item.detail.spriteId))
            {
                item.detail.spriteId = GetDefaultSpriteGuid();
            }

            return item.detail;
        }

        private static QuickBackgroundDetail FindDetail(List<QuickBackgroundDataSingle> list, string guid)
        {
            return list.Find(v => v.guid == guid)?.detail;
        }

        private static QuickBackgroundDetail CreateDefaultDetail()
        {
            return new QuickBackgroundDetail
            {
                spriteId = GetDefaultSpriteGuid()
            };
        }

        private static void WriteDetailFromObject(QuickBackgroundDetail detail, GameObject background)
        {
            Transform imageTransform = GetImageTransform(background);
            RectTransform rectTransform = imageTransform.GetComponent<RectTransform>();
            Image image = imageTransform.GetComponent<Image>();
            if (rectTransform == null || image == null)
            {
                throw new InvalidOperationException("UXQuickBackgroundImage is missing RectTransform or Image component.");
            }

            detail.isOpen = isOpen;
            detail.position = imageTransform.localPosition;
            detail.rotation = imageTransform.rotation.eulerAngles;
            detail.scale = imageTransform.localScale;
            detail.size = rectTransform.sizeDelta;
            detail.color = image.color;
            if (image.sprite != null && image.sprite.texture != null)
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(image.sprite.texture, out detail.spriteId, out long _);
            }
        }

        private static Sprite LoadSprite(string spriteGuid)
        {
            string guid = string.IsNullOrEmpty(spriteGuid) ? GetDefaultSpriteGuid() : spriteGuid;
            string spritePath = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        }

        private static string GetDefaultSpriteGuid()
        {
            return AssetDatabase.AssetPathToGUID($"{ThunderFireUIToolConfig.IconPath}QuickBackgroundDefault.png");
        }

        private static string GetStageAssetPath(PrefabStage stage)
        {
#if UNITY_2020_1_OR_NEWER
            return stage.assetPath;
#else
            return stage.prefabAssetPath;
#endif
        }
    }
}
#endif
