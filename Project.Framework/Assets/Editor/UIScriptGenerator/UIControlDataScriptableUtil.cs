using System;
using System.Collections.Generic;
using System.IO;
using GameLogic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UnityFramework.Editor.UI
{
    internal static class UIControlDataScriptableUtil
    {
        public static string DatabaseRoot => ScriptGeneratorSetting.GetScriptableDatabaseRoot();

        public static void SerializeToControlDataScriptable(UIControlData rootUIControlData, string rootAssetPath)
        {
            if (rootUIControlData == null)
            {
                throw new ArgumentNullException(nameof(rootUIControlData));
            }

            EnsureDirectory(DatabaseRoot);
            var databasePath = GetDatabaseAssetPath(rootUIControlData.ClassName);
            var database = LoadOrCreateDatabase(databasePath);
            database.ownerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(rootAssetPath);
            database.rootNode = SerializeNode(rootUIControlData, rootUIControlData.transform);

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void DeserializeToUIControlData(UIControlDataScriptable database, string rootAssetPath)
        {
            if (database == null || database.rootNode == null)
            {
                Debug.LogError("Deserialize failed: database or root node is null.");
                return;
            }

            var rootObject = ResolveRootObject(rootAssetPath);
            if (rootObject == null)
            {
                Debug.LogError($"Deserialize failed: root prefab not found. path={rootAssetPath}");
                return;
            }

            var rootControl = DeserializeNode(database.rootNode, rootObject.transform, rootObject.transform);
            UIAutoGenEditorTools.RefreshHierarchy(rootControl);
            EditorUtility.SetDirty(rootObject);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void CollectMissingScriptsRecursively(GameObject go, List<GameObject> missingList)
        {
            if (go == null || missingList == null)
            {
                return;
            }

            if (HasMissingScripts(go))
            {
                missingList.Add(go);
            }

            foreach (Transform child in go.transform)
            {
                CollectMissingScriptsRecursively(child.gameObject, missingList);
            }
        }

        private static bool HasMissingScripts(GameObject go)
        {
            foreach (var component in go.GetComponents<UnityEngine.Component>())
            {
                if (component == null)
                {
                    return true;
                }
            }

            return false;
        }

        private static UIControlDataScriptable LoadOrCreateDatabase(string databasePath)
        {
            var database = AssetDatabase.LoadAssetAtPath<UIControlDataScriptable>(databasePath);
            if (database != null)
            {
                return database;
            }

            database = ScriptableObject.CreateInstance<UIControlDataScriptable>();
            AssetDatabase.CreateAsset(database, databasePath);
            return database;
        }

        private static string GetDatabaseAssetPath(string className)
        {
            return $"{DatabaseRoot}/{className}.asset";
        }

        private static void EnsureDirectory(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var parts = folderPath.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }

        private static GameObject ResolveRootObject(string rootAssetPath)
        {
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null && stage.assetPath == rootAssetPath)
            {
                return stage.prefabContentsRoot;
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(rootAssetPath);
        }

        private static UIControlDataNode SerializeNode(UIControlData data, Transform root)
        {
            var node = new UIControlDataNode
            {
                path = UIAutoGenEditorTools.GetRelativePath(data.transform, root, false),
                generateType = data.GenerateType,
                uiLayer = data.UILayer,
                isAutoCreateCtrl = data.IsAutoCreateCtrl,
                isJustOne = data.IsJustOne,
                className = data.ClassName,
                variableName = data.VariableName,
                parentClassName = data.ParentClassName,
            };

            FillControlNodes(data, root, node.controls);
            FillSubNodes(data, root, node.subUIs);
            return node;
        }

        private static void FillControlNodes(UIControlData data, Transform root, List<UIControlDataCtrlNode> target)
        {
            foreach (var ctrl in data.CtrlItemDatas)
            {
                var unityObject = ctrl.targets is { Length: > 0 } ? ctrl.targets[0] : null;
                var targetTransform = ResolveControlTransform(unityObject, ctrl.type);
                var targetPath = targetTransform == null ? string.Empty : UIAutoGenEditorTools.GetRelativePath(targetTransform, root, false);

                target.Add(new UIControlDataCtrlNode
                {
                    name = ctrl.name,
                    type = ctrl.type,
                    parentClassName = ctrl.parentClassName,
                    targetPath = targetPath,
                });
            }
        }

        private static void FillSubNodes(UIControlData data, Transform root, List<UIControlDataNode> target)
        {
            foreach (var sub in data.SubUIItemDatas)
            {
                if (sub?.subUIData == null)
                {
                    continue;
                }

                target.Add(SerializeNode(sub.subUIData, root));
            }
        }

        private static Transform ResolveControlTransform(UnityEngine.Object target, string typeName)
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

        private static UIControlData DeserializeNode(UIControlDataNode node, Transform root, Transform parent)
        {
            var target = string.IsNullOrEmpty(node.path) ? parent : root.Find(node.path);
            if (target == null)
            {
                Debug.LogError($"Deserialize failed: node path not found. path={node.path}");
                return null;
            }

            if (!target.TryGetComponent<UIControlData>(out var data))
            {
                data = target.gameObject.AddComponent<UIControlData>();
            }

            ApplyNodeMeta(node, data);
            DeserializeControls(node, data, root);
            DeserializeSubs(node, data, root);
            return data;
        }

        private static void ApplyNodeMeta(UIControlDataNode node, UIControlData data)
        {
            data.GenerateType = node.generateType;
            data.UILayer = node.uiLayer;
            data.IsAutoCreateCtrl = node.isAutoCreateCtrl;
            data.IsJustOne = node.isJustOne;
            data.ClassName = node.className;
            data.VariableName = node.variableName;
            data.ParentClassName = node.parentClassName;
            data.CtrlItemDatas.Clear();
            data.SubUIItemDatas.Clear();
        }

        private static void DeserializeControls(UIControlDataNode node, UIControlData data, Transform root)
        {
            foreach (var ctrlNode in node.controls)
            {
                var ctrl = new CtrlItemData
                {
                    name = ctrlNode.name,
                    type = ctrlNode.type,
                    parentClassName = ctrlNode.parentClassName,
                };
                ctrl.targets[0] = ResolveTargetObject(root, ctrlNode.targetPath, ctrlNode.type);
                data.CtrlItemDatas.Add(ctrl);
            }
        }

        private static void DeserializeSubs(UIControlDataNode node, UIControlData data, Transform root)
        {
            foreach (var subNode in node.subUIs)
            {
                var subData = DeserializeNode(subNode, root, root);
                if (subData != null)
                {
                    data.AddSubControlData(new SubUIItemData { subUIData = subData });
                }
            }
        }

        private static UnityEngine.Object ResolveTargetObject(Transform root, string targetPath, string typeName)
        {
            if (string.IsNullOrEmpty(targetPath))
            {
                return null;
            }

            var target = root.Find(targetPath);
            if (target == null)
            {
                return null;
            }

            if (typeName == nameof(GameObject) || typeName == nameof(Transform))
            {
                return target.gameObject;
            }

            var type = UIControlTypeResolver.Resolve(typeName);
            return type == null ? null : target.GetComponent(type);
        }
    }
}
