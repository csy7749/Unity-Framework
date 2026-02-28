using System;

namespace GameLogic.Guide
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class PropertyOrderAttribute : Attribute
    {
        public int Order;
        public PropertyOrderAttribute()
        {

        }
        public PropertyOrderAttribute(int order)
        {
            this.Order = order;
        }
    }
}
