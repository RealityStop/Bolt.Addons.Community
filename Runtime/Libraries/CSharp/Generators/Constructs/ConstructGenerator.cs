using System.Collections.Generic;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.ConstructGenerator")]
    public abstract class ConstructGenerator : ICodeGenerator
    {
        private bool _canCompile = true;
        bool ICodeGenerator.CanCompile
        {
            get => _canCompile;
            set => _canCompile = value;
        }

        public virtual bool CanCompile
        {
            get => _canCompile;
            protected set => _canCompile = value;
        }


        public abstract string Generate(int indent);

        public string GenerateClean(int indent)
        {
            return GenerateWithoutStyles(indent);
        }

        public string GenerateWithoutStyles(int indent)
        {
            var output = this.Generate(indent);
            return output.RemoveHighlights().RemoveMarkdown();
        }

        public abstract List<string> Usings();

        string ICodeGenerator.Generate(int indent)
        {
            throw new System.NotImplementedException();
        }

        string ICodeGenerator.GenerateClean(int indent)
        {
            throw new System.NotImplementedException();
        }
    }
}