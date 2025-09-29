using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ExponentialFunctionOfRange))]
    public class ExponentialFunctionOfRangeGenerator : MathFunctionGenerator<ExponentialFunctionOfRange>
    {
        protected override string MethodName => "ExponentialFunctionOfRange";

        public ExponentialFunctionOfRangeGenerator(Unit unit) : base(unit) { }

        protected override IEnumerable<ValueInput> Values()
        {
            yield return Unit.input;
            yield return Unit.minimumRange;
            yield return Unit.maximumRange;
            yield return Unit.minimumValue;
            yield return Unit.exponent;
            yield return Unit.scale;
        }
    }
}