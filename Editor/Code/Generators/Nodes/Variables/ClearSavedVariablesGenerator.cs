using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ClearSavedVariables))]
    public class ClearSavedVarsGenerator : NodeGenerator<ClearSavedVariables>
    {
        public ClearSavedVarsGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return base.GenerateValue(output, data);
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return CodeBuilder.Indent(indent) + CodeBuilder.CallCSharpUtilityMethod(Unit, MakeClickableForThisUnit("ClearSavedVariables")) + MakeClickableForThisUnit(";") + "\n" + GetNextUnit(Unit.Out, data, indent);
        }
    }
}
