using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Utility;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

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

        public override string Generate(int indent)
        {
            if (!useAssemblyQualifiedType && type == null) return "/* Parameter type is null */".WarningHighlight();
            var _attributes = attributes != null && attributes.Count > 0 ? string.Join(" ", attributes.Select(attr => AttributeGenerator.Attribute(attr.GetAttributeType()).AddParameters(attr.parameters).Generate(0))) + " " : string.Empty;
            var _modifier = RuntimeTypeUtility.GetModifierAsString(modifier);
            var _default = isLiteral && !useAssemblyQualifiedType ? type.IsBasic() ? " = " + defaultValue.As().Code(true, true) : " = " + (type.IsStruct() && isLiteral && !type.IsBasic() ? "default".ConstructHighlight() : "null".ConstructHighlight()) : "";
            var parameter = _attributes + (useAssemblyQualifiedType ? _modifier + assemblyQualifiedType + " " + name.VariableHighlight() : _modifier + type.As().CSharpName() + " " + name.LegalMemberName().VariableHighlight()) + _default;
            return parameter;
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
            if (type != typeof(void) && type != typeof(Void) && !type.IsPrimitive)
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