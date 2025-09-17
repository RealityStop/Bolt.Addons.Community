
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Using))]
    public class UsingGenerator : NodeGenerator<Using>
    {
        public UsingGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return base.GenerateValue(output, data);
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            string output = string.Empty;

            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit($"{"using".ConstructHighlight()} (") + GenerateValue(Unit.value, data) + MakeClickableForThisUnit(")") + "\n";
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("{") + "\n";
            output += GetNextUnit(Unit.body, data, indent + 1);
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("}") + "\n";
            output += GetNextUnit(Unit.exit, data, indent);
            return output;
        }
    }
}
