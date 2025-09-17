using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(StrikethroughString))]
    public class StrikethroughStringGenerator : NodeGenerator<StrikethroughString>
    {
        public StrikethroughStringGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeClickableForThisUnit($"$\"<s>{{") + GenerateValue(Unit.Value, data) + MakeClickableForThisUnit("}</s>\"");
        }
    }
}