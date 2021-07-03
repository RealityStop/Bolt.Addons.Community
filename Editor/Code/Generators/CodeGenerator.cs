using Bolt.Addons.Libraries.CSharp;
using Bolt.Addons.Libraries.Humility;
using System;

namespace Bolt.Addons.Community.Code.Editor
{
    [Serializable]
    public abstract class CodeGenerator : Decorator<CodeGenerator, CodeGeneratorAttribute, object>, ICodeGenerator
    {
        public abstract string Generate(int indent);

        public string GenerateClean(int indent)
        {
            return Generate(indent).RemoveHighlights().RemoveMarkdown();
        }
    }

    [Serializable]
    public abstract class CodeGenerator<T> : CodeGenerator
    {
        public T Data => (T)decorated;
    }
}
