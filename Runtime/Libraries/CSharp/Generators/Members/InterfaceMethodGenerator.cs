using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Community.CSharp;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.InterfaceMethodGenerator")]
    public sealed class InterfaceMethodGenerator : ConstructGenerator
    {
        public string name;
        public Type returnType;
        public List<ParameterGenerator> parameters = new List<ParameterGenerator>();

        public override void Generate(CodeWriter writer, ControlGenerationData data)
        {
            if (string.IsNullOrEmpty(name)) return;

            writer.WriteIndented(writer.GetTypeNameHighlighted(returnType) + " " + name.LegalMemberName() + "(");

            for (int i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                if (i != 0) writer.Write(", ");
                parameter.Generate(writer, data);
            }

            writer.WriteEnd();
        }

        internal InterfaceMethodGenerator() { }

        public static InterfaceMethodGenerator Method(string name, Type returnType)
        {
            var method = new InterfaceMethodGenerator();
            method.name = name;
            method.returnType = returnType;
            return method;
        }

        public InterfaceMethodGenerator AddParameter(ParameterGenerator generator)
        {
            parameters.Add(generator);
            return this;
        }

        public override List<string> Usings()
        {
            var usings = new List<string>();
            if (returnType != null && returnType != typeof(void) && !returnType.IsPrimitive) usings.AddRange(returnType.GetAllNamespaces());
            for (int i = 0; i < parameters.Count; i++)
            {
                usings.MergeUnique(parameters[i].Usings());
            }
            return usings;
        }
    }
}