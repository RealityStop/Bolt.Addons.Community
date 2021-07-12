using Bolt.Addons.Community.Generation;
using Bolt.Addons.Libraries.CSharp;
using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Bolt.Addons.Community.Code.Editor
{
    [Serializable]
    [CodeGenerator(typeof(ClassAsset))]
    public sealed class ClassAssetGenerator : MemberTypeAssetGenerator<ClassAsset, ClassFieldDeclaration, ClassMethodDeclaration, ClassConstructorDeclaration>
    {
        protected override TypeGenerator OnGenerateType(ref string output, NamespaceGenerator @namespace)
        {
            var @class = ClassGenerator.Class(RootAccessModifier.Public, ClassModifier.None, Data.title.LegalMemberName(), Data.scriptableObject ? typeof(ScriptableObject) : typeof(object));
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

            for (int i = 0; i < Data.methods.Count; i++)
            {
                if (!string.IsNullOrEmpty(Data.methods[i].name) && Data.methods[i].returnType != null)
                {
                    var method = MethodGenerator.Method(Data.methods[i].scope, Data.methods[i].modifier, Data.methods[i].returnType, Data.methods[i].name);
                    if (Data.methods[i].graph.units.Count > 0)
                    {
                        var unit = Data.methods[i].graph.units[0] as FunctionUnit;
                        method.Body(FunctionUnitGenerator.GetSingleDecorator(unit, unit).GenerateControl(null, new ControlGenerationData(), 0));
                    }
                    @class.AddMethod(method);
                }
            }

            if (@namespace != null)
            {
                @namespace.AddClass(@class);
            }

            return @class;
        }
    }
}
