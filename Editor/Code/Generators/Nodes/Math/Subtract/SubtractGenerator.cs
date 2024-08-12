using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public abstract class SubtractGenerator<T> : NodeGenerator<Subtract<T>>
    {
        public SubtractGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            var minuend = GenerateValue(Unit.minuend, data);
            var subtrahend = GenerateValue(Unit.subtrahend, data);
            return CodeUtility.MakeSelectable(Unit, $"{minuend} - {subtrahend}");
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input.hasValidConnection)
            {
                return GetNextValueUnit(input, data);
            }
            else if (input.hasDefaultValue)
            {
                if(data.GetExpectedType() == typeof(int))
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
