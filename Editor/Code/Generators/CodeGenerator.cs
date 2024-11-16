using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using System;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    public abstract class CodeGenerator : Decorator<CodeGenerator, CodeGeneratorAttribute, object>, ICodeGenerator
    {
        private bool _canCompile = true;
        bool ICodeGenerator.CanCompile
        {
            get => _canCompile;
            set => _canCompile = value;
        }

        public bool CanCompile
        {
            get => _canCompile;
            protected set => _canCompile = value;
        }

        public abstract string Generate(int indent);

        public string GenerateClean(int indent)
        {
            var generatedCode = CodeUtility.RemoveAllToolTipTags(CodeUtility.RemoveAllSelectableTags(CodeUtility.RemoveCustomHighlights(Generate(indent).RemoveHighlights().RemoveMarkdown())));
            var code = "";
            if(generatedCode.Length > 0)
            {
                code = "#pragma warning disable\n";
            }
            code += generatedCode;
            return code;
        }
    }

    [Serializable]
    public abstract class CodeGenerator<T> : CodeGenerator
    {
        public T Data => (T)decorated;
    }
}
