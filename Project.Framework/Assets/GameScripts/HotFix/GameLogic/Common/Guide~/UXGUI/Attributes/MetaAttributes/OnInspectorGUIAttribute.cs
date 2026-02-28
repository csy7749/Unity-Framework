using UnityEngine;
using System;

namespace GameLogic.Guide
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class OnInspectorGUIAttribute : PropertyAttribute
    {
        public OnInspectorGUIAttribute()
        {

        }
    }
}