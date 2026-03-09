using System.Collections.Generic;
using Unity.VisualScripting.Community.CSharp;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.EnumValueGenerator")]
    public sealed class EnumValueGenerator : ConstructGenerator
    {
        public string name = string.Empty;
        public int index = 0;

        public override void Generate(CodeWriter writer, ControlGenerationData data)
        {
            writer.WriteIndented(name + " = " + index.ToString());
        }

        public override List<string> Usings()
        {
            return new List<string>();
        }
    }
}