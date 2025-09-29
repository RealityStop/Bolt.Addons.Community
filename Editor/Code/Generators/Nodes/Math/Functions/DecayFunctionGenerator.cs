using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(DecayFunction))]
    public class DecayFunctionGenerator : MathFunctionGenerator<DecayFunction>
    {
        public DecayFunctionGenerator(Unit unit) : base(unit) { }

        protected override string MethodName => "DecayFunction";

        protected override IEnumerable<ValueInput> Values()
        {
            yield return Unit.input;
            yield return Unit.minimum;
            yield return Unit.decayFactor;
            yield return Unit.scale;
        }
    }
}