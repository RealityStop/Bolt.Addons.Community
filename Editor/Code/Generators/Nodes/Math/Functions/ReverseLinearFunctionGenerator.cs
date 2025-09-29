using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ReverseLinearFunction))]
    public class ReverseLinearFunctionGenerator : MathFunctionGenerator<ReverseLinearFunction>
    {
        public ReverseLinearFunctionGenerator(Unit unit) : base(unit) { }

        protected override string MethodName => "ReverseLinearFunction";

        protected override IEnumerable<ValueInput> Values()
        {
            yield return Unit.input;
            yield return Unit.minimum;
            yield return Unit.maximum;
        }
    }
}