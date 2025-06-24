using UnityEngine;
using System;

namespace UnityFramework
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class OnInspectorGUIAttribute : PropertyAttribute
    {
        public OnInspectorGUIAttribute()
        {

        }
    }
}