using System;
using System.Collections.Generic;
using GameLogic;
using GameLogic.Binding;
using GameLogic.Binding.Binders;
using GameLogic.Binding.Builder;
using GameLogic.Binding.Contexts;
using GameLogic.Contexts;
using UnityEngine;
using UnityFramework;

namespace GameLogic
{
    public static class UIWindowBindingExtension
    {
        private static IBinder _binder;

        public static IBinder Binder
        {
            get
            {
                if (_binder == null)
                    _binder = Context.GetApplicationContext().GetService<IBinder>();

                if (_binder == null)
                    throw new Exception(
                        "Data binding service is not initialized,please create a BindingServiceBundle service before using it.");

                return _binder;
            }
        }

        public static IBindingContext BindingContext(this UIWindow uiWindow)
        {
            if (uiWindow == null || uiWindow.gameObject == null)
                return null;

            if (uiWindow.gameObject.GetComponent<BindingContextLifecycle>() == null)
            {
                uiWindow.gameObject.AddComponent<BindingContextLifecycle>();
            }

            BindingContextLifecycle bindingContextLifecycle =
                uiWindow.transform.GetComponent<BindingContextLifecycle>();
            if (bindingContextLifecycle == null)
                bindingContextLifecycle = uiWindow.gameObject.AddComponent<BindingContextLifecycle>();

            IBindingContext bindingContext = bindingContextLifecycle.BindingContext;
            if (bindingContext == null)
            {
                bindingContext = new BindingContext(uiWindow, Binder);
                bindingContextLifecycle.BindingContext = bindingContext;
            }

            return bindingContext;
        }

        public static BindingSet<TUIWindow, TSource> CreateBindingSet<TUIWindow, TSource>(this TUIWindow uiWindow)
            where TUIWindow : UIWindow
        {
            IBindingContext context = uiWindow.BindingContext();
            return new BindingSet<TUIWindow, TSource>(context, uiWindow);
        }

        public static BindingSet<TUIWindow, TSource> CreateBindingSet<TUIWindow, TSource>(this TUIWindow uiWindow,
            TSource dataContext) where TUIWindow : UIWindow
        {
            IBindingContext context = uiWindow.BindingContext();
            context.DataContext = dataContext;
            return new BindingSet<TUIWindow, TSource>(context, uiWindow);
        }

        public static BindingSet<TUIWindow> CreateBindingSet<TUIWindow>(this TUIWindow uiWindow)
            where TUIWindow : UIWindow
        {
            IBindingContext context = uiWindow.BindingContext();
            return new BindingSet<TUIWindow>(context, uiWindow);
        }

        public static BindingSet CreateSimpleBindingSet(this UIWindow uiWindow)
        {
            IBindingContext context = uiWindow.BindingContext();
            return new BindingSet(context, uiWindow);
        }

        public static void SetDataContext(this UIWindow uiWindow, object dataContext)
        {
            uiWindow.BindingContext().DataContext = dataContext;
        }

        public static object GetDataContext(this UIWindow uiWindow)
        {
            return uiWindow.BindingContext().DataContext;
        }

        public static void AddBinding(this UIWindow uiWindow, BindingDescription bindingDescription)
        {
            uiWindow.BindingContext().Add(uiWindow, bindingDescription);
        }

        public static void AddBindings(this UIWindow uiWindow, IEnumerable<BindingDescription> bindingDescriptions)
        {
            uiWindow.BindingContext().Add(uiWindow, bindingDescriptions);
        }

        public static void AddBinding(this UIWindow uiWindow, IBinding binding)
        {
            uiWindow.BindingContext().Add(binding);
        }

        public static void AddBinding(this UIWindow uiWindow, IBinding binding, object key = null)
        {
            uiWindow.BindingContext().Add(binding, key);
        }

        public static void AddBindings(this UIWindow uiWindow, IEnumerable<IBinding> bindings, object key = null)
        {
            if (bindings == null)
                return;

            uiWindow.BindingContext().Add(bindings, key);
        }

        public static void AddBinding(this UIWindow uiWindow, object target, BindingDescription bindingDescription,
            object key = null)
        {
            uiWindow.BindingContext().Add(target, bindingDescription, key);
        }

        public static void AddBindings(this UIWindow uiWindow, object target,
            IEnumerable<BindingDescription> bindingDescriptions, object key = null)
        {
            uiWindow.BindingContext().Add(target, bindingDescriptions, key);
        }

        public static void AddBindings(this UIWindow uiWindow,
            IDictionary<object, IEnumerable<BindingDescription>> bindingMap, object key = null)
        {
            if (bindingMap == null)
                return;

            IBindingContext context = uiWindow.BindingContext();
            foreach (var kvp in bindingMap)
            {
                context.Add(kvp.Key, kvp.Value, key);
            }
        }

        public static void ClearBindings(this UIWindow uiWindow, object key)
        {
            uiWindow.BindingContext().Clear(key);
        }

        public static void ClearAllBindings(this UIWindow uiWindow)
        {
            uiWindow.BindingContext().Clear();
        }
    }
}