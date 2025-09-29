using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(LinearFunctionOfRange))]
    public class LinearFunctionOfRangeGenerator : MathFunctionGenerator<LinearFunctionOfRange>
    {
        public LinearFunctionOfRangeGenerator(Unit unit) : base(unit) { }

        protected override string MethodName => "LinearFunctionOfRange";

        protected override IEnumerable<ValueInput> Values()
        {
            yield return Unit.input;
            yield return Unit.minInputRange;
            yield return Unit.maxInputRange;
            yield return Unit.minimum;
            yield return Unit.maximum;
        }
    }
}