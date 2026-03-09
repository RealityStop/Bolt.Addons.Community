using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    public abstract class MathFunctionGenerator<TUnit> : NodeGenerator<TUnit> where TUnit : Unit
    {
        protected abstract string MethodName { get; }
        public MathFunctionGenerator(Unit unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.CallCSharpUtilityMethod(MethodName, Values().Select(v => (CodeWriter.MethodParameter)writer.Action(() => GenerateValue(v, data, writer))).ToArray());
        }

        /// <summary>
        /// Make sure to return ports in correct order as method call
        /// </summary>
        protected abstract IEnumerable<ValueInput> Values();
    }
}