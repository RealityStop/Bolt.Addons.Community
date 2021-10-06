using System;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public sealed class InheritsAttribute : Attribute
    {
        public Type type;

        public InheritsAttribute(Type type)
        {
            this.type = type;
        }
    }
}
