using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;
using Unity.VisualScripting.Community.CSharp;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.EventGenerator")]
    public sealed class EventGenerator : MemberGenerator
    {
        private EventGenerator() { }

        public static EventGenerator Event(AccessModifier scope, Type returnType, string name)
        {
            return new EventGenerator()
            {
                scope = scope,
                returnType = returnType,
                name = name
            };
        }

        public override void Generate(CodeWriter writer, ControlGenerationData data)
        {
            writer.WriteIndented((scope.AsString() + " event ").ConstructHighlight() + writer.GetTypeNameHighlighted(returnType) + " " + name.LegalVariableName().VariableHighlight() + ";");
            data.AddLocalNameInScope(name.LegalVariableName(), returnType);
        }

        public override List<string> Usings()
        {
            var usings = new List<string>();

            if (!returnType.Is().PrimitiveStringOrVoid()) usings.Add(returnType.Namespace);

            return usings;
        }
    }
}
