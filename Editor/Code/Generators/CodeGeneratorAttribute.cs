using Unity.VisualScripting.Community.Libraries.Humility;
using System;

namespace Unity.VisualScripting.Community
{
    public sealed class CodeGeneratorAttribute : DecoratorAttribute
    {
        public CodeGeneratorAttribute(Type type) : base(type)
        {
        }
    }
}
