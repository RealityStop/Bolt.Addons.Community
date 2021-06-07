using Bolt.Addons.Libraries.Humility;
using System;

namespace Bolt.Addons.Community.Code.Editor
{
    public sealed class CodeGeneratorAttribute : DecoratorAttribute
    {
        public CodeGeneratorAttribute(Type type) : base(type)
        {
        }
    }
}
