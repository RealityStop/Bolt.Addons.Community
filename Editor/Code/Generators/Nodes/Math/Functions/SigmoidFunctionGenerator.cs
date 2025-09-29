using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(SigmoidFunction))]
    public class SigmoidFunctionGenerator : MathFunctionGenerator<SigmoidFunction>
    {
        public SigmoidFunctionGenerator(Unit unit) : base(unit) { }

        protected override string MethodName => "DecayingSigmoid";

        protected override IEnumerable<ValueInput> Values()
        {
            yield return Unit.input;
            yield return Unit.inflectionPoint;
            yield return Unit.minimum;
            yield return Unit.decayFactor;
            yield return Unit.scale;
        }
    }
}