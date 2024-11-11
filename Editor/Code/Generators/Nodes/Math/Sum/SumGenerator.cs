using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public abstract class SumGenerator<T> : NodeGenerator<T> where T : Unit, IMultiInputUnit
    {
        public SumGenerator(Unit unit) : base(unit)
        {
        }

        bool expectsDefaultType = false;
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            List<string> values = new List<string>();
            if (Unit.multiInputs.Any(input => !input.hasValidConnection))
            {
                expectsDefaultType = true;
            }
            foreach (var item in this.Unit.multiInputs)
            {
                values.Add(GenerateValue(item, data));
            }
            return string.Join(MakeSelectableForThisUnit(" + "), values);
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input.hasValidConnection)
            {
                if (expectsDefaultType)
                    data.SetExpectedType(input.type);
                var connectedCode = GetNextValueUnit(input, data);
                if (expectsDefaultType)
                    data.RemoveExpectedType();
                return connectedCode;
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