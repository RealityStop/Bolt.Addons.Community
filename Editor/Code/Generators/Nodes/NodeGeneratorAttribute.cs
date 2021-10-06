using System;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    public sealed class NodeGeneratorAttribute : DecoratorAttribute
    {
        public NodeGeneratorAttribute(Type type) : base(type)
        {
        }
    }
}