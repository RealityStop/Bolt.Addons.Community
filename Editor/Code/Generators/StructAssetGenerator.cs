using Bolt.Addons.Libraries.CSharp;
using System;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Code.Editor
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

            for (int i = 0; i < Data.fields.Count; i++)
            {
                if (!string.IsNullOrEmpty(Data.fields[i].name) && Data.fields[i].type != null)
                {
                    var field = FieldGenerator.Field(AccessModifier.Public, FieldModifier.None, Data.fields[i].type, Data.fields[i].name);
                    if (Data.serialized)
                    {
                        if (Data.fields[i].inspectable) field.AddAttribute(AttributeGenerator.Attribute<InspectableAttribute>());
                        if (!Data.fields[i].serialized) field.AddAttribute(AttributeGenerator.Attribute<NonSerializedAttribute>());
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
