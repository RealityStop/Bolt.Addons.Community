using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public abstract class DivideGenerator<T> : NodeGenerator<Divide<T>>
    {
        public DivideGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            var dividend =GenerateValue(Unit.dividend, data);
            var divisor = GenerateValue(Unit.divisor, data);
            return CodeUtility.MakeSelectable(Unit, $"{dividend} / {divisor}");
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
                    return int.Parse(unit.defaultValues[input.key].ToString()).As().Code(true, true, true, "", false);
                }
                return unit.defaultValues[input.key].As().Code(true, true, true, "", false);
            }
            else
            {
                return $"/* \"{input.key} Requires Input\" */";
            }
        }
    }
}
