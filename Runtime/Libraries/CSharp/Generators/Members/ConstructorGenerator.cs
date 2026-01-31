using Unity.VisualScripting.Community.Libraries.Humility;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Utility;
using Unity.VisualScripting.Community.CSharp;
using System;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.ConstructorGenerator")]
    public sealed class ConstructorGenerator : MemberBodyGenerator
    {
        private ConstructorModifier modifier;
        public List<(bool useInCall, ParameterGenerator generator)> parameters { get; private set; } = new List<(bool useInCall, ParameterGenerator generator)>();
        private List<AttributeGenerator> attributes = new List<AttributeGenerator>();

        private System.Action<CodeWriter> bodyAction;

        public List<string> callChainParameters = new List<string>();

        private ConstructorInitializer callType;

        private ConstructorGenerator() { }

        /// <summary>
        /// Create a new constructor.
        /// </summary>
        public static ConstructorGenerator Constructor(AccessModifier scope, ConstructorModifier modifier, ConstructorInitializer callType, string name)
        {
            var constructor = new ConstructorGenerator
            {
                scope = scope,
                modifier = modifier,
                name = name,
                callType = callType
            };
            return constructor;
        }

        IDisposable nodeScope = null;

        protected override sealed void GenerateBefore(CodeWriter writer, ControlGenerationData data)
        {
            if (owner != null)
                nodeScope = writer.BeginNode(owner);

            foreach (AttributeGenerator attr in this.attributes)
            {
                attr.Generate(writer, data);
                writer.NewLine();
            }
            var modSpace = modifier == ConstructorModifier.None ? string.Empty : " ";

            if (scope != AccessModifier.None && modifier != ConstructorModifier.Static)
            {
                writer.WriteIndented(scope.AsString().ToLower().ConstructHighlight() + " ");
            }
            else
            {
                writer.WriteIndented();
            }

            writer.Write(modifier.AsString().ConstructHighlight()).Write(modSpace);
            writer.Write(name.LegalMemberName().TypeHighlight()).Write("(");

            if (modifier != ConstructorModifier.Static)
            {
                for (int i = 0; i < this.parameters.Count; i++)
                {
                    this.parameters[i].generator.Generate(writer, data);
                    if (i < this.parameters.Count - 1) writer.Write(", ");
                    if (this.parameters[i].useInCall)
                    {
                        callChainParameters.Add(this.parameters[i].generator.name.VariableHighlight());
                    }
                }
            }

            writer.Write(")");

            string _callChainParameters = string.Join(", ", callChainParameters);
            string callChain = callType != ConstructorInitializer.None && modifier != ConstructorModifier.Static ? $" : {(callType == ConstructorInitializer.Base ? "base" : "this").ConstructHighlight()}(" + _callChainParameters + ")" : string.Empty;

            writer.Write(callChain);
            writer.NewLine();
        }

        protected override sealed void GenerateBody(CodeWriter writer, ControlGenerationData data)
        {
            data.EnterMethod();
            bodyAction?.Invoke(writer);
            data.ExitMethod();
        }

        protected override sealed void GenerateAfter(CodeWriter writer, ControlGenerationData data)
        {
            nodeScope?.Dispose();
        }

        public ConstructorGenerator Body(System.Action<CodeWriter> bodyAction)
        {
            this.bodyAction = bodyAction;
            return this;
        }

        public ConstructorGenerator AddToBody(System.Action<CodeWriter> bodyAction)
        {
            this.bodyAction += bodyAction;
            return this;
        }

        public ConstructorGenerator AddParameter(bool useInCall, ParameterGenerator parameter)
        {
            parameters.Add((useInCall, parameter));
            return this;
        }

        public ConstructorGenerator AddBaseParameters(List<string> parameterNames)
        {
            callChainParameters.AddRange(parameterNames);
            return this;
        }

        public ConstructorGenerator AddBaseParameter(string parameterName)
        {
            callChainParameters.Add(parameterName);
            return this;
        }

        public override List<string> Usings()
        {
            var usings = new List<string>();
            if (returnType != typeof(void) && !returnType.IsPrimitive) usings.Add(returnType.Namespace);
            for (int i = 0; i < parameters.Count; i++)
            {
                if (!parameters[i].generator.type.Is().PrimitiveStringOrVoid()) usings.AddRange(parameters[i].generator.Usings());
            }
            return usings;
        }
    }
}
