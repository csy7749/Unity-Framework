using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameLogic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityFramework.Editor.UI
{
    internal sealed class UGUINodeProviderMenuItemInfo
    {
        public string TypeName;
        public UnityEngine.Object Object;
        public CtrlItemData CtrlItemData;
    }

    internal static class UIControlTypeResolver
    {
        private static readonly Dictionary<string, Type> TypeMap = new Dictionary<string, Type>
        {
            { nameof(GameObject), typeof(GameObject) },
            { nameof(Transform), typeof(Transform) },
            { nameof(RectTransform), typeof(RectTransform) },
            { nameof(Text), typeof(Text) },
            { nameof(Image), typeof(Image) },
            { nameof(RawImage), typeof(RawImage) },
            { nameof(Button), typeof(Button) },
            { nameof(Toggle), typeof(Toggle) },
            { nameof(Slider), typeof(Slider) },
            { nameof(Scrollbar), typeof(Scrollbar) },
            { nameof(ScrollRect), typeof(ScrollRect) },
            { nameof(Dropdown), typeof(Dropdown) },
            { nameof(InputField), typeof(InputField) },
            { nameof(CanvasGroup), typeof(CanvasGroup) },
            { nameof(GridLayoutGroup), typeof(GridLayoutGroup) },
            { nameof(HorizontalLayoutGroup), typeof(HorizontalLayoutGroup) },
            { nameof(VerticalLayoutGroup), typeof(VerticalLayoutGroup) },
        };

        private static bool _initialized;

        public static Type Resolve(string typeName)
        {
            EnsureInitialized();
            TypeMap.TryGetValue(typeName, out var type);
            return type;
        }

        public static List<UGUINodeProviderMenuItemInfo> CollectMenuInfos(GameObject gameObject)
        {
            EnsureInitialized();
            var result = new List<UGUINodeProviderMenuItemInfo>();
            if (gameObject == null)
            {
                return result;
            }

            AddGameObjectEntry(result, gameObject);
            AddComponentEntries(result, gameObject);
            return result;
        }

        private static void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            foreach (var type in LoadComponentTypes())
            {
                TypeMap.TryAdd(type.Name, type);
            }
        }

        private static IEnumerable<Type> LoadComponentTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(SafeGetTypes)
                .Where(t => typeof(UnityEngine.Component).IsAssignableFrom(t) && !t.IsAbstract);
        }

        private static IEnumerable<Type> SafeGetTypes(System.Reflection.Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
            catch
            {
                return Array.Empty<Type>();
            }
        }

        private static void AddGameObjectEntry(List<UGUINodeProviderMenuItemInfo> result, GameObject gameObject)
        {
            result.Add(new UGUINodeProviderMenuItemInfo
            {
                TypeName = nameof(GameObject),
                Object = gameObject,
                CtrlItemData = CreateCtrlItemData(gameObject, nameof(GameObject), gameObject),
            });
        }

        private static void AddComponentEntries(List<UGUINodeProviderMenuItemInfo> result, GameObject gameObject)
        {
            foreach (var component in gameObject.GetComponents<UnityEngine.Component>())
            {
                if (component == null || component is Transform || component is UIControlData)
                {
                    continue;
                }

                var typeName = component.GetType().Name;
                result.Add(new UGUINodeProviderMenuItemInfo
                {
                    TypeName = typeName,
                    Object = component,
                    CtrlItemData = CreateCtrlItemData(gameObject, typeName, component),
                });
            }
        }

        private static CtrlItemData CreateCtrlItemData(GameObject gameObject, string typeName, UnityEngine.Object target)
        {
            return new CtrlItemData
            {
                name = UIAutoGenEditorTools.GetVariableName(gameObject.name, NamingConvention.PascalCase),
                type = typeName,
                targets = new[] { target },
            };
        }
    }
}
