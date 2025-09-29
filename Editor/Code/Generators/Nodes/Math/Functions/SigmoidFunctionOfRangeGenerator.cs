using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(SigmoidFunctionOfRange))]
    public class SigmoidFunctionOfRangeGenerator : MathFunctionGenerator<SigmoidFunctionOfRange>
    {
        public SigmoidFunctionOfRangeGenerator(Unit unit) : base(unit) { }

        protected override string MethodName => "DecayingSigmoidOfRange";

        protected override IEnumerable<ValueInput> Values()
        {
            yield return Unit.input;
            yield return Unit.minimumRange;
            yield return Unit.maximumRange;
            yield return Unit.inflectionPoint;
            yield return Unit.minimum;
            yield return Unit.decayFactor;
            yield return Unit.scale;
        }
    }
}