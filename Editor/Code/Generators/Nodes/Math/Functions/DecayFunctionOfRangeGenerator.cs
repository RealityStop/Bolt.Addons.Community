using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(DecayFunctionOfRange))]
    public class DecayFunctionOfRangeGenerator : MathFunctionGenerator<DecayFunctionOfRange>
    {
        public DecayFunctionOfRangeGenerator(Unit unit) : base(unit) { }

        protected override string MethodName => "DecayFunctionOfRange";

        protected override IEnumerable<ValueInput> Values()
        {
            yield return Unit.input;
            yield return Unit.minimumRange;
            yield return Unit.maximumRange;
            yield return Unit.minimumValue;
            yield return Unit.decayFactor;
            yield return Unit.scale;
        }
    }
}