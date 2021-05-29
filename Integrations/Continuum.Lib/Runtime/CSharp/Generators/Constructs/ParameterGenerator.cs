using Bolt.Addons.Integrations.Continuum.Humility;
using System;
using System.Collections.Generic;

namespace Bolt.Addons.Integrations.Continuum.CSharp
{
    public sealed class ParameterGenerator : ConstructGenerator
    {
        public string name;
        public Type type;
        public string assemblyQualifiedType;
        public bool useAssemblyQualifiedType;

        public bool isLiteral;
        public ParameterModifier modifier;

        public override string Generate(int indent)
        {
            return type == null ? assemblyQualifiedType + " " + name : type.As().CSharpName() + " " + name.LegalMemberName();
        }

        private ParameterGenerator()
        {

        }

        public static ParameterGenerator Parameter(string name, Type type, ParameterModifier modifier, bool isLiteral = false)
        {
            var parameter = new ParameterGenerator();
            parameter.name = name;
            parameter.type = type;
            parameter.modifier = modifier;
            parameter.isLiteral = isLiteral;
            return parameter;
        }

        public static ParameterGenerator Parameter(string name, string assemblyQualifiedType, ParameterModifier modifier, bool isLiteral = false)
        {
            var parameter = new ParameterGenerator();
            parameter.name = name;
            parameter.assemblyQualifiedType = assemblyQualifiedType;
            parameter.useAssemblyQualifiedType = true;
            parameter.modifier = modifier;
            parameter.isLiteral = isLiteral;
            return parameter;
        }

        public string Using()
        {
            return type == typeof(void) || type.IsPrimitive ? string.Empty : type.Namespace;
        }

        public override List<string> Usings()
        {
            var usings = new List<string>();


            return usings;
        }
    }
}