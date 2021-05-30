using Bolt.Addons.Integrations.Continuum.CSharp;
using Bolt.Addons.Integrations.Continuum.Humility;

namespace Bolt.Addons.Community.Code.Editor
{
    public abstract class CodeGenerator : Decorator<CodeGenerator, CodeGeneratorAttribute, object>, ICodeGenerator
    {
        public abstract string Generate(int indent);

        public string GenerateClean(int indent)
        {
            return Generate(indent).RemoveHighlights().RemoveMarkdown();
        }
    }

    public abstract class CodeGenerator<T> : CodeGenerator
    {
        public T Data => (T)decorated;
    }
}
