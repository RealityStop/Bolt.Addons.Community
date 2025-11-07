using System;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
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