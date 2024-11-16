
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
            var trueData = new ControlGenerationData(data);
            var falseData = new ControlGenerationData(data);
            if (Unit.trueOutput.hasValidConnection)
            {
                output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit("if".ControlHighlight() + " (" + "CSharpUtility".TypeHighlight() + $".Chance(") + GenerateValue(Unit.value, data) + MakeSelectableForThisUnit("))");
                output += "\n" + CodeBuilder.Indent(indent) + MakeSelectableForThisUnit("{") + "\n";
                output += GetNextUnit(Unit.trueOutput, trueData, indent + 1);
                output += "\n" + CodeBuilder.Indent(indent) + MakeSelectableForThisUnit("}") + "\n";
                output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit("else".ControlHighlight());
                output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit("{") + "\n";
                output += GetNextUnit(Unit.falseOutput, falseData, indent + 1);
                output += "\n" + CodeBuilder.Indent(indent) + MakeSelectableForThisUnit("}") + "\n";
            }
            else if (!Unit.trueOutput.hasValidConnection && Unit.falseOutput.hasValidConnection)
            {
                output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit("if".ControlHighlight() + " (!" + "CSharpUtility".TypeHighlight() + $".Chance(") + GenerateValue(Unit.value, data) + MakeSelectableForThisUnit("))");
                output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit("{");
                output += GetNextUnit(Unit.falseOutput, falseData, indent + 1);
                output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit("}");
            }

            if (data.mustReturn)
            {
                if (trueData.hasReturned)
                    data.hasReturned = true;
                else if (falseData.hasReturned)
                    data.hasReturned = true;
            }
            return output;
        }
    }
}
