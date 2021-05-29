﻿using Bolt.Addons.Integrations.Continuum.Humility;
using System;
using System.Collections.Generic;

namespace Bolt.Addons.Integrations.Continuum.CSharp
{
    public sealed class InterfaceMethodGenerator : ConstructGenerator
    {
        public string name;
        public Type returnType;
        public List<ParameterGenerator> parameters = new List<ParameterGenerator>();

        public override string Generate(int indent)
        {
            if (string.IsNullOrEmpty(name)) { return string.Empty; }

            var output = returnType.As().CSharpName() + " " + name.LegalMemberName() + "(";
            for (int i = 0; i < parameters.Count; i++)
            {
                output += parameters[i].Generate(indent);
            }
            output += ");";

            return output;
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
            if (returnType != null && returnType != typeof(void) && !returnType.IsPrimitive) usings.Add(returnType.Namespace);
            for (int i = 0; i < parameters.Count; i++)
            {
                usings.MergeUnique(parameters[i].Usings());
            }
            return usings;
        }
    }
}