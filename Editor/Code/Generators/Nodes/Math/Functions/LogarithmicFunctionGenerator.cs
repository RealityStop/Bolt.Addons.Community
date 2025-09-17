using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(LogarithmicFunction))]
    public class LogarithmicFunctionGenerator : MathFunctionGenerator<LogarithmicFunction>
    {
        public LogarithmicFunctionGenerator(Unit unit) : base(unit) { }

        protected override string MethodName => "LogarithmicFunction";

        protected override IEnumerable<ValueInput> Values()
        {
            yield return Unit.input;
            yield return Unit.minimum;
            yield return Unit.exponent;
            yield return Unit.scale;
        }
    }
}