using Bolt.Addons.Libraries.CSharp;
using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Bolt.Addons.Community.Code.Editor
{
    [CodeGenerator(typeof(CSharpObject))]
    [Serializable]
    public sealed class CSharpObjectGenerator : CodeGenerator<CSharpObject>
    {
        public override string Generate(int indent)
        {
            if (decorated  != null)
            {
                var output = string.Empty;
                NamespaceGenerator @namespace = null;
                ClassGenerator @class = null;
                StructGenerator @struct = null;

                if (string.IsNullOrEmpty(Data.title)) return output;

                if (!string.IsNullOrEmpty(Data.category))
                {
                    @namespace = NamespaceGenerator.Namespace(Data.category);
                }

                if (Data.objectType == ObjectType.Class)
                {
                    @class = ClassGenerator.Class(RootAccessModifier.Public, ClassModifier.None, Data.title.LegalMemberName(), Data.scriptableObject ? typeof(ScriptableObject) : typeof(object));
                    if (Data.definedEvent) @class.ImplementInterface(typeof(IDefinedEvent));
                    if (Data.inspectable) @class.AddAttribute(AttributeGenerator.Attribute<InspectableAttribute>());
                    if (Data.serialized) @class.AddAttribute(AttributeGenerator.Attribute<SerializableAttribute>());
                    if (Data.includeInSettings) @class.AddAttribute(AttributeGenerator.Attribute<IncludeInSettingsAttribute>().AddParameter(true));
                    if (Data.scriptableObject) @class.AddAttribute(AttributeGenerator.Attribute<CreateAssetMenuAttribute>().AddParameter("menuName", Data.menuName).AddParameter("fileName", Data.fileName).AddParameter("order", Data.order));

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
                            @class.AddField(field);
                        }
                    }

                    if (@namespace != null)
                    {
                        @namespace.AddClass(@class);
                    }
                }
                else
                {
                    @struct = StructGenerator.Struct(RootAccessModifier.Public, StructModifier.None, Data.title.LegalMemberName());
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
                }

                if (Data.lastCompiledName != Data.GetFullTypeName())
                {
                    @class.AddAttribute(AttributeGenerator.Attribute<RenamedFromAttribute>().AddParameter(Data.lastCompiledName));
                }

                if (@namespace != null) return @namespace.Generate(indent);
                return Data.objectType == ObjectType.Class ? @class.Generate(indent) : @struct.Generate(indent);
            }

            return string.Empty;
        }
    }
}
