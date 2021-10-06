using System;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public abstract class DecoratorAttribute : Attribute
    {
        public Type type;

        public DecoratorAttribute(Type type)
        {
            this.type = type;
        }
    }
}
