using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [CodeGenerator(typeof(StructAsset))]
    public sealed class StructAssetGenerator : MemberTypeAssetGenerator<StructAsset, StructFieldDeclaration, StructMethodDeclaration, StructConstructorDeclaration>
    {
        protected override TypeGenerator OnGenerateType(ref string output, NamespaceGenerator @namespace)
        {
            var @struct = StructGenerator.Struct(RootAccessModifier.Public, StructModifier.None, Data.title.LegalMemberName());
            if (Data.definedEvent) @struct.ImplementInterface(typeof(IDefinedEvent));
            if (Data.inspectable) @struct.AddAttribute(AttributeGenerator.Attribute<InspectableAttribute>());
            if (Data.serialized) @struct.AddAttribute(AttributeGenerator.Attribute<SerializableAttribute>());
            if (Data.includeInSettings) @struct.AddAttribute(AttributeGenerator.Attribute<IncludeInSettingsAttribute>().AddParameter(true));

            for (int i = 0; i < Data.variables.Count; i++)
            {
                if (!string.IsNullOrEmpty(Data.variables[i].name) && Data.variables[i].type != null)
                {
                    var field = FieldGenerator.Field(AccessModifier.Public, FieldModifier.None, Data.variables[i].type, Data.variables[i].name);
                    if (Data.serialized)
                    {
                        if (Data.variables[i].inspectable) field.AddAttribute(AttributeGenerator.Attribute<InspectableAttribute>());
                        if (!Data.variables[i].serialized) field.AddAttribute(AttributeGenerator.Attribute<NonSerializedAttribute>());
                    }
                    @struct.AddField(field);
                }
            }

            if (@namespace != null)
            {
                @namespace.AddStruct(@struct);
            }

            return @struct;
        }
    }
}
