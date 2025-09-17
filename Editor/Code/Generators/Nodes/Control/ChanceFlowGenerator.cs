
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ChanceFlow))]
    public class ChanceFlowGenerator : NodeGenerator<ChanceFlow>
    {
        public ChanceFlowGenerator(Unit unit) : base(unit) { }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = "";
            if (Unit.trueOutput.hasValidConnection)
            {
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("if".ControlHighlight() + " (" + "CSharpUtility".TypeHighlight() + $".Chance(") + GenerateValue(Unit.value, data) + MakeClickableForThisUnit("))");
                output += "\n" + CodeBuilder.Indent(indent) + MakeClickableForThisUnit("{") + "\n";
                data.NewScope();
                output += GetNextUnit(Unit.trueOutput, data, indent + 1);
                data.ExitScope();
                output += "\n" + CodeBuilder.Indent(indent) + MakeClickableForThisUnit("}") + "\n";
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("else".ControlHighlight());
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("{") + "\n";
                data.NewScope();
                output += GetNextUnit(Unit.falseOutput, data, indent + 1);
                data.ExitScope();
                output += "\n" + CodeBuilder.Indent(indent) + MakeClickableForThisUnit("}") + "\n";
            }
            else if (!Unit.trueOutput.hasValidConnection && Unit.falseOutput.hasValidConnection)
            {
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("if".ControlHighlight() + " (!" + "CSharpUtility".TypeHighlight() + $".Chance(") + GenerateValue(Unit.value, data) + MakeClickableForThisUnit("))");
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("{");
                data.NewScope();
                output += GetNextUnit(Unit.falseOutput, data, indent + 1);
                data.ExitScope();
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("}");
            }

            return output;
        }
    }
}
