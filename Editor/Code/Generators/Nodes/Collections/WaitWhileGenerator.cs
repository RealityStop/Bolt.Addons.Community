
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(WaitWhileUnit))]
    public class WaitWhileGenerator : NodeGenerator<WaitWhileUnit>
    {
        public WaitWhileGenerator(Unit unit) : base(unit)
        {
            NameSpace = "UnityEngine";
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return base.GenerateValue(output, data);
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return MakeSelectableForThisUnit("yield return".ControlHighlight() + " new ".ConstructHighlight() + "WaitWhile".TypeHighlight() + "(() => ") + GenerateValue(Unit.condition, data) + MakeSelectableForThisUnit(");");
        }
    }
}
