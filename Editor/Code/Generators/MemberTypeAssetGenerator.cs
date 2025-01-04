using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using Unity.VisualScripting;
using System.Linq;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    public abstract class MemberTypeAssetGenerator<TMemberTypeAsset, TFieldDeclaration, TMethodDeclaration, TConstructorDeclaration> : CodeGenerator<TMemberTypeAsset>
        where TMemberTypeAsset : MemberTypeAsset<TFieldDeclaration, TMethodDeclaration, TConstructorDeclaration>
        where TFieldDeclaration : FieldDeclaration
        where TMethodDeclaration : MethodDeclaration
        where TConstructorDeclaration : ConstructorDeclaration
    {
        protected abstract TypeGenerator OnGenerateType(ref string output, NamespaceGenerator @namespace);

        public override string Generate(int indent)
        {
            if (decorated != null)
            {
                var output = string.Empty;
                NamespaceGenerator @namespace = NamespaceGenerator.Namespace(string.Empty);
                if (string.IsNullOrEmpty(Data.title)) return output;

                if (!string.IsNullOrEmpty(Data.category))
                {
                    @namespace = NamespaceGenerator.Namespace(Data.category);
                }

                TypeGenerator type = OnGenerateType(ref output, @namespace);
                ClassGenerator classType = type as ClassGenerator;
                StructGenerator structType = type as StructGenerator;

                foreach (var name in Data.lastCompiledNames)
                {
                    if (!string.IsNullOrEmpty(Data.GetFullTypeName()) && name != Data.GetFullTypeName())
                    {
                        if (!string.IsNullOrEmpty(name))
                        {
                            if (classType != null) classType.AddAttribute(AttributeGenerator.Attribute<RenamedFromAttribute>().AddParameter(name));
                            if (structType != null) structType.AddAttribute(AttributeGenerator.Attribute<RenamedFromAttribute>().AddParameter(name));
                        }

                    }
                }

                return @namespace.Generate(indent);
            }

            return string.Empty;
        }
    }
}
