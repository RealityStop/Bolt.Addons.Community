using System;
using Bolt.Addons.Libraries.Humility;

namespace Bolt.Addons.Community.Generation
{
    public sealed class UnitGeneratorAttribute : DecoratorAttribute
    {
        public UnitGeneratorAttribute(Type type) : base(type)
        {
        }
    }
}