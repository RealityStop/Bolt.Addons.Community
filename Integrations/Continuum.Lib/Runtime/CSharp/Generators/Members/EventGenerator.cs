using Bolt.Addons.Integrations.Continuum.Humility;
using System;
using System.Collections.Generic;

namespace Bolt.Addons.Integrations.Continuum.CSharp
{
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

        public override string Generate(int indent)
        {
            return CodeBuilder.Indent(indent) + scope.AsString() + " event " + returnType.Name + " " + name + ";";
        }

        public override List<string> Usings()
        {
            var usings = new List<string>();

            if (!returnType.Is().PrimitiveStringOrVoid()) usings.Add(returnType.Namespace);

            return usings;
        }
    }
}
