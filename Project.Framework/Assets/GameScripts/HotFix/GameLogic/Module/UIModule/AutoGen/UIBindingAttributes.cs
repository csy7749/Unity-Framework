using System;

namespace GameLogic
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ControlBindingAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SubUIBindingAttribute : Attribute
    {
    }

    public interface IBindableUI
    {
    }
}
