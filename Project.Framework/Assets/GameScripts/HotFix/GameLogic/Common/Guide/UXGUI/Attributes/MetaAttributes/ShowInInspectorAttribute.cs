using UnityEngine;
using System;

namespace GameLogic.Guide
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ShowInInspectorAttribute : PropertyAttribute
    {
        public ShowInInspectorAttribute()
        {
        }
    }
}