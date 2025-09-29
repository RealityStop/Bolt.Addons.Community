using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Between))]
    public class BetweenGenerator : NodeGenerator<Between>
    {
        public BetweenGenerator(Unit unit) : base(unit) { NameSpaces = "Unity.VisualScripting.Community"; }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return CodeBuilder.CallCSharpUtilityMethod(Unit, MakeClickableForThisUnit("IsWithin"), GenerateValue(Unit.input, data), GenerateValue(Unit.min, data), GenerateValue(Unit.max, data));
        }
    }
}
