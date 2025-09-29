using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ExponentialFunction))]
    public class ExponentialFunctionGenerator : MathFunctionGenerator<ExponentialFunction>
    {
        protected override string MethodName => "ExponentialFunction";
        public ExponentialFunctionGenerator(Unit unit) : base(unit) { }
        protected override IEnumerable<ValueInput> Values()
        {
            yield return Unit.input;
            yield return Unit.minimum;
            yield return Unit.exponent;
            yield return Unit.scale;
        }
    }
}