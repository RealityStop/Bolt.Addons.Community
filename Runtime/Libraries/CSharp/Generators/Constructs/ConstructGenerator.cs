using System.Collections.Generic;
using Unity.VisualScripting.Community.CSharp;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.ConstructGenerator")]
    public abstract class ConstructGenerator : ICodeGenerator
    {
        protected Unit owner;
        public abstract void Generate(CodeWriter writer, ControlGenerationData data);

        public string GenerateClean(CodeWriter writer, ControlGenerationData data)
        {
            return GenerateWithoutStyles(writer, data);
        }

        public string GenerateWithoutStyles(CodeWriter writer, ControlGenerationData data)
        {
            Generate(writer, data);
            return writer.ToString();
        }

        public void SetOwner(Unit owner)
        {
            this.owner = owner;
        }

        public abstract List<string> Usings();
    }
}