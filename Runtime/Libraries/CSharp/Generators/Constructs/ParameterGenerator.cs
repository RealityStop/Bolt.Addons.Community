using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Utility;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.CSharp;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.ParameterGenerator")]
    public sealed class ParameterGenerator : ConstructGenerator
    {
        public string name;
        public Type type;
        public string assemblyQualifiedType;
        public bool useAssemblyQualifiedType;
        public bool isLiteral;
        public ParameterModifier modifier;
        public object defaultValue;

        public List<AttributeDeclaration> attributes = new List<AttributeDeclaration>();

        public override void Generate(CodeWriter writer, ControlGenerationData data)
        {
            if (!useAssemblyQualifiedType && type == null)
            {
                writer.Error("Parameter type is null");
                return;
            }
            if (attributes != null && attributes.Count > 0)
            {
                foreach (var item in attributes.Select(attr => AttributeGenerator.Attribute(attr.GetAttributeType()).AddParameters(attr.parameters)))
                {
                    writer.Write(" ");
                    item.Generate(writer, data);
                }
                writer.Write(" ");
            }

            var _modifier = RuntimeTypeUtility.GetModifierAsString(modifier);
            var _default = isLiteral && !useAssemblyQualifiedType ? type.IsBasic() ? " = " + defaultValue.As().Code(true, true) : " = " + (type.IsStruct() && isLiteral && !type.IsBasic() ? "default".ConstructHighlight() : "null".ConstructHighlight()) : "";
            writer.Write((useAssemblyQualifiedType ? _modifier + assemblyQualifiedType + " " + name.VariableHighlight() : _modifier + writer.GetTypeNameHighlighted(type) + " " + name.LegalMemberName().VariableHighlight()) + _default);
        }

        private ParameterGenerator()
        {

        }

        public static ParameterGenerator Parameter(string name, Type type, ParameterModifier modifier, bool isLiteral = false, object defaultValue = null)
        {
            var parameter = new ParameterGenerator();
            parameter.name = name;
            parameter.type = type;
            parameter.modifier = modifier;
            parameter.isLiteral = isLiteral;
            parameter.defaultValue = defaultValue;
            parameter.useAssemblyQualifiedType = false;
            return parameter;
        }

        public static ParameterGenerator Parameter(string name, Type type, ParameterModifier modifier, List<AttributeDeclaration> attributes, bool isLiteral = false, object defaultValue = null)
        {
            var parameter = new ParameterGenerator();
            parameter.name = name;
            parameter.type = type;
            parameter.modifier = modifier;
            parameter.isLiteral = isLiteral;
            parameter.defaultValue = defaultValue;
            parameter.useAssemblyQualifiedType = false;
            parameter.attributes = attributes;
            return parameter;
        }

        public static ParameterGenerator Parameter(string name, string stringType, Type actualType, ParameterModifier modifier, bool isLiteral = false, object defaultValue = null)
        {
            var parameter = new ParameterGenerator();
            parameter.name = name;
            parameter.assemblyQualifiedType = stringType;
            parameter.useAssemblyQualifiedType = true;
            parameter.modifier = modifier;
            parameter.isLiteral = isLiteral;
            parameter.type = actualType;
            parameter.defaultValue = defaultValue;
            return parameter;
        }

        public string Using()
        {
            if (useAssemblyQualifiedType || type == null) return string.Empty;
            return type == typeof(void) || type == typeof(Void) || type.IsPrimitive ? string.Empty : type.Namespace;
        }

        public override List<string> Usings()
        {
            var usings = new List<string>();
            if (type == null) return usings;
            if (!type.Is().NullOrVoid() && !type.IsPrimitive)
            {
                if (type.Namespace != "Unity.VisualScripting.Community.Generics")
                    usings.Add(type.Namespace);
            }
            foreach (var attribute in attributes)
            {
                usings.MergeUnique(attribute.Usings());
            }
            return usings;
        }
    }
}