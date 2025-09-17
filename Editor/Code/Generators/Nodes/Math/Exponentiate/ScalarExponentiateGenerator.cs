using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ScalarExponentiate))]
    public class ScalarExponentiateGenerator : NodeGenerator<ScalarExponentiate>
    {
        public ScalarExponentiateGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            var @base = GenerateValue(Unit.@base, data);
            var exponent = GenerateValue(Unit.exponent, data);
            return CodeBuilder.StaticCall(Unit, typeof(Mathf), "Pow", true, @base, exponent);
        }
    }
}