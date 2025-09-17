using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ResetSavedVariables))]
    public class ResetSavedVariablesGenerator : NodeGenerator<ResetSavedVariables>
    {
        public ResetSavedVariablesGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return base.GenerateValue(output, data);
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            string output = string.Empty;
            foreach (var arg in Unit.arguments)
            {
                output += CodeBuilder.Indent(indent) + CodeBuilder.CallCSharpUtilityMethod(Unit, MakeClickableForThisUnit("ResetSavedVariable"), GenerateValue(arg, data)) + MakeClickableForThisUnit(";") + "\n";
            }
            return output + GetNextUnit(Unit.OnReset, data, indent);
        }
    }
}
