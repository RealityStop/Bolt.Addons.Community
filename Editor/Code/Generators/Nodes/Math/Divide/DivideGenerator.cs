using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public abstract class DivideGenerator<T> : NodeGenerator<Divide<T>>
    {
        public DivideGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            var shouldExpectDivisorType = IsSourceLiteral(Unit.divisor, out var divisorType) && Unit.dividend.hasValidConnection;
            if (shouldExpectDivisorType)
            {
                data.SetExpectedType(divisorType);
            }
            var dividend = GenerateValue(Unit.dividend, data);
            if (shouldExpectDivisorType)
            {
                data.RemoveExpectedType();
            }
            var shouldExpectDividendType = IsSourceLiteral(Unit.dividend, out var dividendType) && Unit.divisor.hasValidConnection;
            if (shouldExpectDividendType)
            {
                data.SetExpectedType(dividendType);
            }
            var divisor = GenerateValue(Unit.divisor, data);
            if (shouldExpectDividendType)
            {
                data.RemoveExpectedType();
            }
            return MakeClickableForThisUnit("(") + dividend + MakeClickableForThisUnit(" / ") + divisor + MakeClickableForThisUnit(")");
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input.hasValidConnection)
            {
                return GetNextValueUnit(input, data);
            }
            else if (input.hasDefaultValue)
            {
                if (data.GetExpectedType() == typeof(int))
                {
                    return int.Parse(unit.defaultValues[input.key].ToString()).As().Code(true, Unit, true, true, "", false);
                }
                return unit.defaultValues[input.key].As().Code(true, Unit, true, true, "", false);
            }
            else
            {
                return $"/* \"{input.key} Requires Input\" */".WarningHighlight();
            }
        }
    }
}
