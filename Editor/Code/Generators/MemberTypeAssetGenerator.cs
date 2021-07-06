using Bolt.Addons.Libraries.CSharp;
using System;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Code.Editor
{
    [Serializable]
    public abstract class MemberTypeAssetGenerator<TMemberTypeAsset, TFieldDeclaration> : CodeGenerator<TMemberTypeAsset> 
        where TMemberTypeAsset : MemberTypeAsset<TFieldDeclaration>
        where TFieldDeclaration : FieldDeclaration
    {
        protected abstract TypeGenerator OnGenerateType(ref string output, NamespaceGenerator @namespace);

        public override string Generate(int indent)
        {
            if (decorated  != null)
            {
                var output = string.Empty;
                NamespaceGenerator @namespace = null;
                if (string.IsNullOrEmpty(Data.title)) return output;

                if (!string.IsNullOrEmpty(Data.category))
                {
                    @namespace = NamespaceGenerator.Namespace(Data.category);
                }

                TypeGenerator type = OnGenerateType(ref output, @namespace);
                ClassGenerator classType = type as ClassGenerator;
                StructGenerator structType = type as StructGenerator;

                if (Data.lastCompiledName != Data.GetFullTypeName())
                {
                    if (classType != null) classType.AddAttribute(AttributeGenerator.Attribute<RenamedFromAttribute>().AddParameter(Data.lastCompiledName));
                    if (structType != null) structType.AddAttribute(AttributeGenerator.Attribute<RenamedFromAttribute>().AddParameter(Data.lastCompiledName));
                }

                if (@namespace != null) return @namespace.Generate(indent);
                return classType == null ? structType.Generate(indent) : classType.Generate(indent);
            }

            return string.Empty;
        }
    }
}
