using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Utility;
using System.Linq;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.ParameterGenerator")]
    public sealed class ParameterGenerator : ConstructGenerator
    {
        public string name;
        public Type type;
        public string assemblyQualifiedType;
        public bool useAssemblyQualifiedType;
        public bool isParameters;
        public bool isLiteral;
        public ParameterModifier modifier;
        public object defaultValue;

        public List<AttributeDeclaration> attributes;

        public override string Generate(int indent)
        {
            if (!useAssemblyQualifiedType && type == null) return "/* Parameter type is null */".WarningHighlight();
            var _attributes = attributes != null && attributes.Count > 0 ? string.Join(" ", attributes.Select(attr => $"[{attr.GetAttributeType().As().CSharpName()}]")) + " " : string.Empty;
            var param = isParameters ? "params ".ConstructHighlight() : string.Empty;
            var _modifier = modifier != ParameterModifier.None ? modifier.AsString().ConstructHighlight() + " " : string.Empty;
            var _default = isLiteral ? " = " + defaultValue.As().Code(true, true) : "";
            var parameter = _attributes + (useAssemblyQualifiedType ? param + _modifier + assemblyQualifiedType + " " + name.VariableHighlight() : param + _modifier + type.As().CSharpName() + " " + name.LegalMemberName().VariableHighlight()) + _default;
            return parameter;
        }

        private ParameterGenerator()
        {

        }

        public static ParameterGenerator Parameter(string name, Type type, ParameterModifier modifier, bool isLiteral = false, bool isParameters = false, object defaultValue = null)
        {
            var parameter = new ParameterGenerator();
            parameter.name = name;
            parameter.type = type;
            parameter.modifier = modifier;
            parameter.isLiteral = isLiteral;
            parameter.defaultValue = defaultValue;
            if (modifier == ParameterModifier.None)
                parameter.isParameters = isParameters;
            parameter.useAssemblyQualifiedType = false;
            return parameter;
        }

        public static ParameterGenerator Parameter(string name, Type type, ParameterModifier modifier, List<AttributeDeclaration> attributes, bool isLiteral = false, bool isParameters = false, object defaultValue = null)
        {
            var parameter = new ParameterGenerator();
            parameter.name = name;
            parameter.type = type;
            parameter.modifier = modifier;
            parameter.isLiteral = isLiteral;
            parameter.defaultValue = defaultValue;
            if (modifier == ParameterModifier.None)
                parameter.isParameters = isParameters;
            parameter.useAssemblyQualifiedType = false;
            parameter.attributes = attributes;
            return parameter;
        }

        public static ParameterGenerator Parameter(string name, string assemblyQualifiedType, ParameterModifier modifier, bool isLiteral = false, bool isParameters = false, object defaultValue = null)
        {
            var parameter = new ParameterGenerator();
            parameter.name = name;
            parameter.assemblyQualifiedType = assemblyQualifiedType;
            parameter.useAssemblyQualifiedType = true;
            parameter.modifier = modifier;
            parameter.isLiteral = isLiteral;
            parameter.defaultValue = defaultValue;
            if (modifier == ParameterModifier.None)
                parameter.isParameters = isParameters;
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
            return usings;
        }
    }
}