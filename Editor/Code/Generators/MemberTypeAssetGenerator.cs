using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using Unity.VisualScripting;
using System.Linq;

namespace Unity.VisualScripting.Community.CSharp
{
    [Serializable]
    public abstract class MemberTypeAssetGenerator<TMemberTypeAsset, TFieldDeclaration, TMethodDeclaration, TConstructorDeclaration> : CodeGenerator<TMemberTypeAsset>
        where TMemberTypeAsset : MemberTypeAsset<TFieldDeclaration, TMethodDeclaration, TConstructorDeclaration>
        where TFieldDeclaration : FieldDeclaration
        where TMethodDeclaration : MethodDeclaration
        where TConstructorDeclaration : ConstructorDeclaration
    {
        protected abstract TypeGenerator OnGenerateType(NamespaceGenerator @namespace);

        public override void Generate(CodeWriter writer, ControlGenerationData data)
        {
            if (decorated != null)
            {
                NamespaceGenerator @namespace = NamespaceGenerator.Namespace(string.Empty);
                if (string.IsNullOrEmpty(Data.title)) return;

                if (!string.IsNullOrEmpty(Data.category))
                {
                    @namespace = NamespaceGenerator.Namespace(Data.category);
                }

                TypeGenerator type = OnGenerateType(@namespace);
                ClassGenerator classType = type as ClassGenerator;
                StructGenerator structType = type as StructGenerator;

                string fullName = Data.GetFullTypeName();
                
                foreach (var name in Data.lastCompiledNames ?? new System.Collections.Generic.List<string>())
                {
                    if (!string.IsNullOrEmpty(fullName) && name != fullName)
                    {
                        if (!string.IsNullOrEmpty(name))
                        {
                            if (classType != null) classType.AddAttribute(AttributeGenerator.Attribute<RenamedFromAttribute>().AddParameter(name));
                            if (structType != null) structType.AddAttribute(AttributeGenerator.Attribute<RenamedFromAttribute>().AddParameter(name));
                        }

                    }
                }

                @namespace.Generate(writer, data);
                data.Dispose();
            }
        }
    }
}
