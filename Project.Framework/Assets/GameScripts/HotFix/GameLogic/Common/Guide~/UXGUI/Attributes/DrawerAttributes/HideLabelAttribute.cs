using UnityEngine;
using System;

namespace GameLogic.Guide
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class HideLabelAttribute : PropertyAttribute
    {
        public HideLabelAttribute()
        {
        }
    }
}
