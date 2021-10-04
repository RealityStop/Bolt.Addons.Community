using System;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    public sealed class UnitGeneratorAttribute : DecoratorAttribute
    {
        public UnitGeneratorAttribute(Type type) : base(type)
        {
        }
    }
}