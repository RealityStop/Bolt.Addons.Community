using Unity.VisualScripting.Community.Libraries.Humility;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.ConstructorGenerator")]
    public sealed class ConstructorGenerator : MemberBodyGenerator
    {
        private ConstructorModifier modifier;
        public List<(bool useInCall, ParameterGenerator generator)> parameters { get; private set; } = new List<(bool useInCall, ParameterGenerator generator)>();
        private List<AttributeGenerator> attributes = new List<AttributeGenerator>();
        private string body;

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

        protected override sealed string GenerateBefore(int indent)
        {
            var attributes = string.Empty;
            foreach (AttributeGenerator attr in this.attributes)
            {
                attributes += attr.Generate(indent) + "\n";
            }
            var modSpace = modifier == ConstructorModifier.None ? string.Empty : " ";
            var parameters = string.Empty;

            if (modifier != ConstructorModifier.Static)
            {
                for (int i = 0; i < this.parameters.Count; i++)
                {
                    parameters += this.parameters[i].generator.Generate(0);
                    if (i < this.parameters.Count - 1) parameters += ", ";
                    if (this.parameters[i].useInCall)
                    {
                        callChainParameters.Add(this.parameters[i].generator.name.VariableHighlight());
                    }
                }
            }
            
            string _callChainParameters = string.Join(", ", callChainParameters);
            string callChain = callType != ConstructorInitializer.None && modifier != ConstructorModifier.Static ? $" : {(callType == ConstructorInitializer.Base ? "base" : "this").ConstructHighlight()}(" + _callChainParameters + ")" : string.Empty;
            return attributes + CodeBuilder.Indent(indent) + (scope == AccessModifier.None || modifier == ConstructorModifier.Static ? "" : scope.AsString().ToLower().ConstructHighlight() + " ") + modifier.AsString().ConstructHighlight() + modSpace + name.LegalMemberName().TypeHighlight() + "(" + parameters + ")" + callChain;
        }

        protected override sealed string GenerateBody(int indent)
        {
            return string.IsNullOrEmpty(body) ? string.Empty : body.Contains("\n") ? body.Replace("\n", "\n" + CodeBuilder.Indent(indent)).Insert(0, CodeBuilder.Indent(indent)) : CodeBuilder.Indent(indent) + body;
        }

        protected override sealed string GenerateAfter(int indent)
        {
            return string.Empty;
        }

        public ConstructorGenerator Body(string body)
        {
            this.body = body;
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
                if (!usings.Contains(parameters[i].generator.type.Namespace) && !parameters[i].generator.type.Is().PrimitiveStringOrVoid()) usings.Add(parameters[i].generator.type.Namespace);
            }
            return usings;
        }
    }
}
