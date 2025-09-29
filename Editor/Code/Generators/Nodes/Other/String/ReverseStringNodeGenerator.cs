using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ReverseStringNode))]
    public class ReverseStringNodeGenerator : NodeGenerator<ReverseStringNode>
    {
        public ReverseStringNodeGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return CodeBuilder.CallCSharpUtilityMethod(Unit, MakeClickableForThisUnit("ReverseString"), GenerateValue(Unit.input, data));
        }
    }
}