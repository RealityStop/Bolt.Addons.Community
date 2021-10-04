using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;

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

        public override string Generate(int indent)
        {
            if (!useAssemblyQualifiedType && type == null) return "/* Parameter type is null */".WarningHighlight();
            var param = isParameters ? "params ".ConstructHighlight() : string.Empty;
            return useAssemblyQualifiedType ? param + assemblyQualifiedType + " " + name : param + type.As().CSharpName() + " " + name.LegalMemberName();
        }

        private ParameterGenerator()
        {

        }

        public static ParameterGenerator Parameter(string name, Type type, ParameterModifier modifier, bool isLiteral = false, bool isParameters = false)
        {
            var parameter = new ParameterGenerator();
            parameter.name = name;
            parameter.type = type;
            parameter.modifier = modifier;
            parameter.isLiteral = isLiteral;
            parameter.isParameters = isParameters;
            parameter.useAssemblyQualifiedType = false;
            return parameter;
        }

        public static ParameterGenerator Parameter(string name, string assemblyQualifiedType, ParameterModifier modifier, bool isLiteral = false, bool isParameters = false)
        {
            var parameter = new ParameterGenerator();
            parameter.name = name;
            parameter.assemblyQualifiedType = assemblyQualifiedType;
            parameter.useAssemblyQualifiedType = true;
            parameter.modifier = modifier;
            parameter.isLiteral = isLiteral;
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