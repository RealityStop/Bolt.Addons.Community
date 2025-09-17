using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(SizeString))]
    public class SizeStringGenerator : NodeGenerator<SizeString>
    {
        public SizeStringGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeClickableForThisUnit($"$\"<size={Unit.size.As().Code(false)}>{{") + GenerateValue(Unit.Value, data) + MakeClickableForThisUnit("}</size>\"");
        }
    }
}