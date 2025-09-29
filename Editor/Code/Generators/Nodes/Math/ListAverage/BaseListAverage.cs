using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public abstract class BaseListAverage<TUnit, T> : NodeGenerator<TUnit> where TUnit : Unit
    {
        public BaseListAverage(Unit unit) : base(unit) { }
        protected abstract ValueInput GetNumbersInput();
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            data.SetExpectedType(typeof(T));
            var numbersCode = GenerateValue(GetNumbersInput(), data);
            data.RemoveExpectedType();

            return CodeBuilder.CallCSharpUtilityMethod(Unit, MakeClickableForThisUnit("CalculateListAverage"), numbersCode);
        }
    }
}