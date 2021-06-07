using System.Collections.Generic;

namespace Bolt.Addons.Libraries.CSharp
{
    public sealed class EnumValueGenerator : ConstructGenerator
    {
        public string name = string.Empty;
        public int index = 0;

        public override string Generate(int indent)
        {
            return CodeBuilder.Indent(indent) + name + " = " + index.ToString();
        }

        public override List<string> Usings()
        {
            return new List<string>();
        }
    }
}