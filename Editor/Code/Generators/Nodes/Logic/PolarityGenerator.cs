using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Polarity))]
    public class PolarityGenerator : NodeGenerator<Polarity>
    {
        public PolarityGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (output == Unit.positive)
            {
                return GenerateValue(Unit.input, data) + MakeClickableForThisUnit($" > {"0".NumericHighlight()}");
            }
            else if (output == Unit.negative)
            {
                return GenerateValue(Unit.input, data) + MakeClickableForThisUnit($" < {"0".NumericHighlight()}");
            }
            else if (output == Unit.zero)
            {
                NameSpaces = "UnityEngine";
                return MakeClickableForThisUnit($"{"Mathf".TypeHighlight()}.Approximately(") + GenerateValue(Unit.input, data) + MakeClickableForThisUnit($", {"0f".NumericHighlight()}");
            }
            return base.GenerateValue(output, data);
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }
    }
}
