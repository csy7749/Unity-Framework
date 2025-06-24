using System;

namespace UnityFramework
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ReorderableListAttribute : SpecialCaseDrawerAttribute
    {
    }
}
