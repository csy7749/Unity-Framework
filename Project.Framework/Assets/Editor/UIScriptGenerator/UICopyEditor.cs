#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using GameLogic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UnityFramework.Editor.UI
{
    public static class UICopyEditor
    {
        private const string ViewToken = "UIXXXView";
        private const string AutoToken = "UIXXXAuto";
        private const string NamespaceToken = "#NAMESPACE#";
        private const string ProviderToken = "//InitUGUINodeProviderType";
        private const string ControlToken = "//UIControlData";
        private const string BindToken = "//UIBind";
        private const string RemoveToken = "//UIRemove";
        private const string EventToken = "//Event";

        public static void CopyUIByUIControlData(GameObject gameObject, UIControlData data, UIControlData parentData = null)
        {
            if (gameObject == null || data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var outputDir = GetOutputDirectory(data);
            Directory.CreateDirectory(outputDir);
            WriteViewFile(outputDir, data);
            WriteAutoFile(outputDir, data);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void GenerateAllMonoByData(UIControlData data, bool includeChildren = true)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            GenerateMonoByData(data);
            if (!includeChildren)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return;
            }

            foreach (var sub in data.SubUIItemDatas)
            {
                if (sub?.subUIData == null)
                {
                    continue;
                }

                GenerateAllMonoByData(sub.subUIData, true);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static UIControlData BindingAllUGUINodeProvider(GameObject rootObject, UIControlData rootData, UIControlData currentData)
        {
            if (rootObject == null || rootData == null || currentData == null)
            {
                throw new ArgumentNullException(nameof(currentData));
            }

            var currentRoot = rootData;
            BindingUGUINodeProvider(rootObject, currentData, null, ref currentRoot);
            EditorUtility.SetDirty(rootObject);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return currentRoot;
        }

        public static bool IsBindingAllUGUINodeProvider(UIControlData data)
        {
            if (data == null)
            {
                return false;
            }

            var providerType = ResolveProviderType(data.ClassName);
            if (providerType == null || !data.TryGetComponent(providerType, out var component))
            {
                return true;
            }

            var hasMissing = HasMissingBinding(component, data);
            if (hasMissing)
            {
                return true;
            }

            foreach (var sub in data.SubUIItemDatas)
            {
                if (sub?.subUIData == null)
                {
                    continue;
                }

                if (IsBindingAllUGUINodeProvider(sub.subUIData))
                {
                    return true;
                }
            }

            return false;
        }

        private static void WriteViewFile(string outputDir, UIControlData data)
        {
            var filePath = Path.Combine(outputDir, $"{data.ClassName}_View.cs");
            if (File.Exists(filePath))
            {
                return;
            }

            var templatePath = data.GenerateType == UIAutoGenerateType.Window ? "UIViewTemplate.txt" : "ItemViewTemplate.txt";
            var content = ReadTemplate(templatePath);
            content = content.Replace(NamespaceToken, ScriptGeneratorSetting.GetUINameSpace());
            content = content.Replace(ViewToken, data.ClassName);
            content = content.Replace(AutoToken, $"{data.ClassName}_Auto");
            File.WriteAllText(filePath, content, Encoding.UTF8);
        }

        private static void WriteAutoFile(string outputDir, UIControlData data)
        {
            var templatePath = data.GenerateType == UIAutoGenerateType.Window ? "UIAutoTemplate.txt" : "ItemAutoTemplate.txt";
            var content = ReadTemplate(templatePath);
            content = content.Replace(NamespaceToken, ScriptGeneratorSetting.GetUINameSpace());
            content = content.Replace(AutoToken, $"{data.ClassName}_Auto");
            content = content.Replace(ProviderToken, $"{data.ClassName}_UGUINodeProvider");
            content = content.Replace(ControlToken, BuildControlProperties(data));
            content = content.Replace(BindToken, BuildBindStatements(data));
            content = content.Replace(RemoveToken, BuildRemoveStatements(data));
            content = content.Replace(EventToken, BuildEventMethods(data));
            content = BuildWindowAttribute(content, data);

            var filePath = Path.Combine(outputDir, $"{data.ClassName}_Auto.cs");
            File.WriteAllText(filePath, content, Encoding.UTF8);
        }

        private static string BuildWindowAttribute(string content, UIControlData data)
        {
            if (data.GenerateType != UIAutoGenerateType.Window)
            {
                return content;
            }

            content = content.Replace("UILayer.Bottom", $"UILayer.{data.UILayer}");
            return content.Replace("Window_XXXUI", data.ClassName);
        }

        private static string BuildControlProperties(UIControlData data)
        {
            var builder = new StringBuilder();
            foreach (var ctrl in data.CtrlItemDatas)
            {
                builder.AppendLine($"        protected {ctrl.type} {ctrl.name} => _uGUINodeProvider.{ctrl.name};");
            }

            foreach (var sub in data.SubUIItemDatas)
            {
                if (sub?.subUIData == null)
                {
                    continue;
                }
                builder.AppendLine($"        protected {sub.typeName}_UGUINodeProvider {sub.name} => _uGUINodeProvider.{sub.name};");
            }

            return builder.ToString();
        }

        private static string BuildBindStatements(UIControlData data)
        {
            var builder = new StringBuilder();
            foreach (var ctrl in data.CtrlItemDatas)
            {
                AppendBindStatement(builder, ctrl);
            }
            return builder.ToString();
        }

        private static string BuildRemoveStatements(UIControlData data)
        {
            var builder = new StringBuilder();
            foreach (var ctrl in data.CtrlItemDatas)
            {
                AppendRemoveStatement(builder, ctrl);
            }
            return builder.ToString();
        }

        private static string BuildEventMethods(UIControlData data)
        {
            var builder = new StringBuilder();
            foreach (var ctrl in data.CtrlItemDatas)
            {
                AppendEventMethod(builder, ctrl);
            }
            return builder.ToString();
        }

        private static void AppendBindStatement(StringBuilder builder, CtrlItemData ctrl)
        {
            var callback = GetEventCallback(ctrl);
            if (!string.IsNullOrEmpty(callback))
            {
                builder.AppendLine($"            {ctrl.name}.{callback};");
            }

            AppendSpecialBindStatement(builder, ctrl);
        }

        private static void AppendRemoveStatement(StringBuilder builder, CtrlItemData ctrl)
        {
            var callback = GetRemoveCallback(ctrl);
            if (!string.IsNullOrEmpty(callback))
            {
                builder.AppendLine($"            {ctrl.name}.{callback};");
            }

            AppendSpecialRemoveStatement(builder, ctrl);
        }

        private static void AppendEventMethod(StringBuilder builder, CtrlItemData ctrl)
        {
            var methodName = GetMethodName(ctrl.name);
            switch (GetEventKind(ctrl))
            {
                case ControlEventKind.Button:
                    builder.AppendLine($"        private void OnClick{methodName}()");
                    builder.AppendLine("        {");
                    builder.AppendLine("        }");
                    break;
                case ControlEventKind.Toggle:
                    builder.AppendLine($"        private void OnToggle{methodName}Changed(bool isOn)");
                    builder.AppendLine("        {");
                    builder.AppendLine("        }");
                    break;
                case ControlEventKind.Slider:
                    builder.AppendLine($"        private void OnSlider{methodName}Changed(float value)");
                    builder.AppendLine("        {");
                    builder.AppendLine("        }");
                    break;
            }

            AppendSpecialEventMethod(builder, ctrl, methodName);
        }

        private static string GetEventCallback(CtrlItemData ctrl)
        {
            var methodName = GetMethodName(ctrl.name);
            return GetEventKind(ctrl) switch
            {
                ControlEventKind.Button => $"onClick.AddListener(OnClick{methodName})",
                ControlEventKind.Toggle => $"onValueChanged.AddListener(OnToggle{methodName}Changed)",
                ControlEventKind.Slider => $"onValueChanged.AddListener(OnSlider{methodName}Changed)",
                _ => string.Empty,
            };
        }

        private static string GetRemoveCallback(CtrlItemData ctrl)
        {
            var methodName = GetMethodName(ctrl.name);
            return GetEventKind(ctrl) switch
            {
                ControlEventKind.Button => $"onClick.RemoveListener(OnClick{methodName})",
                ControlEventKind.Toggle => $"onValueChanged.RemoveListener(OnToggle{methodName}Changed)",
                ControlEventKind.Slider => $"onValueChanged.RemoveListener(OnSlider{methodName}Changed)",
                _ => string.Empty,
            };
        }

        private static void AppendSpecialBindStatement(StringBuilder builder, CtrlItemData ctrl)
        {
            var methodName = GetMethodName(ctrl.name);
            switch (ctrl.type)
            {
                case nameof(SuperText):
                    if (ctrl.targets is { Length: > 0 } && ctrl.targets[0] is SuperText superText && !superText.ignoreLocalization)
                    {
                        builder.AppendLine($"            {ctrl.name}.SetLanguage();");
                    }
                    break;
                case nameof(SuperToggle):
                    builder.AppendLine($"            {ctrl.name}.SetOnToggleItemClick(On{methodName}ToggleItemClick);");
                    break;
                case nameof(SuperPageScrollView):
                    builder.AppendLine($"            {ctrl.name}.InitList(On{methodName}CellValid, On{methodName}FocusCell, On{methodName}GetItem, this);");
                    break;
                case nameof(UILoopScroll):
                case nameof(UILoopScrollAndToggle):
                    builder.AppendLine($"            {ctrl.name}.InitListView(On{methodName}CellValid, On{methodName}GetItem, this);");
                    break;
                case nameof(UILoopScrollMul):
                    builder.AppendLine($"            {ctrl.name}.InitListView(On{methodName}CellValid, On{methodName}GetItem, On{methodName}GetItemIdx, this);");
                    break;
            }
        }

        private static void AppendSpecialRemoveStatement(StringBuilder builder, CtrlItemData ctrl)
        {
            switch (ctrl.type)
            {
                case nameof(SuperToggle):
                case nameof(SuperPageScrollView):
                case nameof(UILoopScroll):
                case nameof(UILoopScrollMul):
                case nameof(UILoopScrollAndToggle):
                    builder.AppendLine($"            {ctrl.name}.Clear();");
                    break;
            }
        }

        private static void AppendSpecialEventMethod(StringBuilder builder, CtrlItemData ctrl, string methodName)
        {
            switch (ctrl.type)
            {
                case nameof(SuperToggle):
                    builder.AppendLine($"        private void On{methodName}ToggleItemClick(int index)");
                    builder.AppendLine("        {");
                    builder.AppendLine("        }");
                    break;
                case nameof(SuperPageScrollView):
                    builder.AppendLine($"        private GameObject On{methodName}GetItem()");
                    builder.AppendLine("        {");
                    builder.AppendLine("            return null;");
                    builder.AppendLine("        }");
                    builder.AppendLine($"        private void On{methodName}CellValid(int index, GameObject item)");
                    builder.AppendLine("        {");
                    builder.AppendLine("        }");
                    builder.AppendLine($"        private void On{methodName}FocusCell(GameObject item, bool focus)");
                    builder.AppendLine("        {");
                    builder.AppendLine("        }");
                    break;
                case nameof(UILoopScroll):
                case nameof(UILoopScrollAndToggle):
                    builder.AppendLine($"        private GameObject On{methodName}GetItem()");
                    builder.AppendLine("        {");
                    builder.AppendLine("            return null;");
                    builder.AppendLine("        }");
                    builder.AppendLine($"        private void On{methodName}CellValid(GameObject item, int index)");
                    builder.AppendLine("        {");
                    builder.AppendLine("        }");
                    break;
                case nameof(UILoopScrollMul):
                    builder.AppendLine($"        private GameObject On{methodName}GetItem(int itemIdx)");
                    builder.AppendLine("        {");
                    builder.AppendLine("            return null;");
                    builder.AppendLine("        }");
                    builder.AppendLine($"        private int On{methodName}GetItemIdx(int index)");
                    builder.AppendLine("        {");
                    builder.AppendLine("            return index;");
                    builder.AppendLine("        }");
                    builder.AppendLine($"        private void On{methodName}CellValid(GameObject item, int index)");
                    builder.AppendLine("        {");
                    builder.AppendLine("        }");
                    break;
            }
        }

        private static ControlEventKind GetEventKind(CtrlItemData ctrl)
        {
            var resolvedType = UIControlTypeResolver.Resolve(ctrl.type);
            if (resolvedType != null)
            {
                if (typeof(Button).IsAssignableFrom(resolvedType))
                {
                    return ControlEventKind.Button;
                }

                if (typeof(Toggle).IsAssignableFrom(resolvedType))
                {
                    return ControlEventKind.Toggle;
                }

                if (typeof(Slider).IsAssignableFrom(resolvedType))
                {
                    return ControlEventKind.Slider;
                }
            }

            return ctrl.type switch
            {
                nameof(Button) => ControlEventKind.Button,
                nameof(Toggle) => ControlEventKind.Toggle,
                nameof(Slider) => ControlEventKind.Slider,
                _ => ControlEventKind.None,
            };
        }

        private enum ControlEventKind
        {
            None = 0,
            Button = 1,
            Toggle = 2,
            Slider = 3,
        }

        private static string GetMethodName(string sourceName)
        {
            return UIAutoGenEditorTools.GetVariableName(sourceName, NamingConvention.PascalCase);
        }

        private static void GenerateMonoByData(UIControlData data)
        {
            var outputDir = GetOutputDirectory(data);
            Directory.CreateDirectory(outputDir);
            var filePath = Path.Combine(outputDir, $"{data.ClassName}_UGUINodeProvider.cs");
            var content = BuildProviderContent(data);
            File.WriteAllText(filePath, content, Encoding.UTF8);
        }

        private static string BuildProviderContent(UIControlData data)
        {
            var builder = new StringBuilder();
            builder.AppendLine("using GameLogic;");
            builder.AppendLine();
            builder.AppendLine("#if UNITY_EDITOR");
            builder.AppendLine($"[UnityEditor.CustomEditor(typeof({data.ClassName}_UGUINodeProvider))]");
            builder.AppendLine($"public class {data.ClassName}_UGUINodeProviderEditor : UnityEditor.Editor");
            builder.AppendLine("{");
            builder.AppendLine("    public override void OnInspectorGUI()");
            builder.AppendLine("    {");
            builder.AppendLine("        DrawDefaultInspector();");
            builder.AppendLine("    }");
            builder.AppendLine("}");
            builder.AppendLine("#endif");
            builder.AppendLine();
            builder.AppendLine($"public class {data.ClassName}_UGUINodeProvider : UIControlData");
            builder.AppendLine("{");
            AppendProviderFields(builder, data);
            builder.AppendLine("}");
            return builder.ToString();
        }

        private static void AppendProviderFields(StringBuilder builder, UIControlData data)
        {
            foreach (var ctrl in data.CtrlItemDatas)
            {
                var type = UIControlTypeResolver.Resolve(ctrl.type);
                if (type == null)
                {
                    Debug.LogError($"Unknown control type: {ctrl.type} in {data.ClassName}");
                    continue;
                }

                builder.AppendLine($"    public {NormalizeTypeName(type)} {ctrl.name};");
            }

            foreach (var sub in data.SubUIItemDatas)
            {
                if (sub?.subUIData == null)
                {
                    continue;
                }

                builder.AppendLine($"    public {sub.typeName}_UGUINodeProvider {sub.name};");
            }
        }

        private static string NormalizeTypeName(Type type)
        {
            return type.FullName?.Replace("+", ".") ?? type.Name;
        }

        private static void BindingUGUINodeProvider(GameObject gameObject, UIControlData data, UIControlData parent, ref UIControlData root)
        {
            var providerType = ResolveProviderType(data.ClassName);
            if (providerType == null)
            {
                Debug.LogError($"Binding failed: provider type not found for {data.ClassName}.");
                return;
            }

            var providerData = EnsureProviderComponent(gameObject, data, providerType, parent, ref root);
            if (providerData == null)
            {
                return;
            }

            foreach (var sub in new List<SubUIItemData>(providerData.SubUIItemDatas))
            {
                if (sub?.subUIData == null)
                {
                    continue;
                }

                BindingUGUINodeProvider(sub.subUIData.gameObject, sub.subUIData, providerData, ref root);
            }

            SetBindingValues(providerData, providerData);
        }

        private static UIControlData EnsureProviderComponent(GameObject gameObject, UIControlData data, Type providerType, UIControlData parent, ref UIControlData root)
        {
            if (gameObject.TryGetComponent(providerType, out var existing))
            {
                return existing as UIControlData;
            }

            var payload = data.CapturePayload();
            UnityEngine.Object.DestroyImmediate(data);
            var provider = gameObject.AddComponent(providerType) as UIControlData;
            if (provider == null)
            {
                Debug.LogError($"Binding failed: can not add provider {providerType.Name}.");
                return null;
            }

            provider.ApplyPayload(payload);
            parent?.ReplaceSubUIControlData(data, provider);
            if (root == data)
            {
                root = provider;
            }
            return provider;
        }

        private static void SetBindingValues(object component, UIControlData data)
        {
            foreach (var field in component.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                if (typeof(UIControlData).IsAssignableFrom(field.FieldType))
                {
                    BindSubField(component, data, field);
                    continue;
                }

                BindControlField(component, data, field);
            }
        }

        private static void BindSubField(object component, UIControlData data, FieldInfo field)
        {
            foreach (var sub in data.SubUIItemDatas)
            {
                if (sub?.subUIData != null && sub.name == field.Name)
                {
                    field.SetValue(component, sub.subUIData);
                    return;
                }
            }
        }

        private static void BindControlField(object component, UIControlData data, FieldInfo field)
        {
            foreach (var ctrl in data.CtrlItemDatas)
            {
                if (ctrl.name != field.Name)
                {
                    continue;
                }

                var value = ResolveFieldValue(field.FieldType, ctrl.targets);
                if (value != null)
                {
                    field.SetValue(component, value);
                }
                return;
            }
        }

        private static object ResolveFieldValue(Type fieldType, UnityEngine.Object[] targets)
        {
            var target = targets is { Length: > 0 } ? targets[0] : null;
            if (target == null)
            {
                return null;
            }

            if (fieldType == typeof(Transform) && target is GameObject go)
            {
                return go.transform;
            }

            if (fieldType.IsInstanceOfType(target))
            {
                return target;
            }

            if (target is UnityEngine.Component component && fieldType.IsInstanceOfType(component))
            {
                return component;
            }

            return null;
        }

        private static bool HasMissingBinding(UnityEngine.Component provider, UIControlData data)
        {
            foreach (var field in provider.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                var value = field.GetValue(provider);
                if (value == null)
                {
                    return true;
                }
            }
            return false;
        }

        private static Type ResolveProviderType(string className)
        {
            var providerTypeName = $"{className}_UGUINodeProvider";
            return FindTypeByName(providerTypeName);
        }

        private static Type FindTypeByName(string typeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(typeName);
                if (type != null)
                {
                    return type;
                }

                foreach (var candidate in SafeGetTypes(assembly))
                {
                    if (candidate.Name == typeName)
                    {
                        return candidate;
                    }
                }
            }
            return null;
        }

        private static IEnumerable<Type> SafeGetTypes(System.Reflection.Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types;
            }
            catch
            {
                return Array.Empty<Type>();
            }
        }

        private static string GetOutputDirectory(UIControlData data)
        {
            var root = ScriptGeneratorSetting.GetResolvedCodePath();
            if (data.GenerateType == UIAutoGenerateType.Window)
            {
                return Path.Combine(root, data.ClassName);
            }

            return Path.Combine(root, "Item", data.ClassName);
        }

        private static string ReadTemplate(string fileName)
        {
            var templateRoot = ScriptGeneratorSetting.GetTemplateRoot().TrimEnd('/');
            var assetPath = $"{templateRoot}/{fileName}";
            var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            if (textAsset == null)
            {
                throw new FileNotFoundException($"Template not found: {assetPath}");
            }

            return textAsset.text;
        }
    }
}
#endif
