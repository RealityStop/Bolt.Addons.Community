using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using System;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    public abstract class CodeGenerator : Decorator<CodeGenerator, CodeGeneratorAttribute, object>, ICodeGenerator, ICreateGenerationData
    {

        public ControlGenerationData data { get; protected set; }

        public virtual ControlGenerationData GetGenerationData(bool newIfDisposed = true)
        {
            if (data == null || (newIfDisposed && data.isDisposed))
            {
                data = CreateGenerationData();
            }
            return data;
        }

        public abstract ControlGenerationData CreateGenerationData();
        
        public abstract string Generate(int indent);

        public string GenerateClean(int indent)
        {
            var generatedCode = CodeUtility.CleanCode(Generate(indent).RemoveHighlights().RemoveMarkdown());
            return generatedCode;
        }

    }

    [Serializable]
    public abstract class CodeGenerator<T> : CodeGenerator
    {
        public T Data => (T)decorated;
    }
}
