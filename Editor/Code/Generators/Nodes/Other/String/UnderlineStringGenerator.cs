using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(UnderlineString))]
    public class UnderlineStringGenerator : NodeGenerator<UnderlineString>
    {
        public UnderlineStringGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeClickableForThisUnit($"$\"<u>{{") + GenerateValue(Unit.Value, data) + MakeClickableForThisUnit("}</u>\"");
        }
    }
}