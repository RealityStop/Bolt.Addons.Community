using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

namespace Unity.VisualScripting.Community
{
    public abstract class MathFunctionGenerator<TUnit> : NodeGenerator<TUnit> where TUnit : Unit
    {
        protected abstract string MethodName { get; }
        public MathFunctionGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return CodeBuilder.CallCSharpUtilityMethod(Unit, MakeClickableForThisUnit(MethodName), Values().Select(v => GenerateValue(v, data)).ToArray());
        }

        /// <summary>
        /// Make sure to return ports in correct order as method call
        /// </summary>
        protected abstract IEnumerable<ValueInput> Values();
    }
}