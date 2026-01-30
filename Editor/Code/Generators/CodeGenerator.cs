using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using System;

namespace Unity.VisualScripting.Community.CSharp
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
        
        public abstract void Generate(CodeWriter writer, ControlGenerationData data);

        public string GenerateClean(CodeWriter writer, ControlGenerationData data)
        {
            Generate(writer, data);
            return writer.ToString();
        }

    }

    [Serializable]
    public abstract class CodeGenerator<T> : CodeGenerator
    {
        public T Data => (T)decorated;
    }
}
