using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ColorString))]
    public class ColorStringGenerator : NodeGenerator<ColorString>
    {
        public ColorStringGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeClickableForThisUnit($"$\"<color={Unit.color.As().Code(false)}>{{") + GenerateValue(Unit.Value, data) + MakeClickableForThisUnit("}</color>\"");
        }
    }
}