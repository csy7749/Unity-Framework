using UnityEngine;
using System;

namespace UnityFramework
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ShowInInspectorAttribute : PropertyAttribute
    {
        public ShowInInspectorAttribute()
        {
        }
    }
}