using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Libraries.CSharp;

[NodeGenerator(typeof(ConvertNode))]
public class ConvertNodeGenerator : NodeGenerator<ConvertNode>
{
    public ConvertNodeGenerator(Unit unit) : base(unit)
    {
    }

    public override string GenerateValue(ValueOutput output, ControlGenerationData data)
    {
        if (data.GetExpectedType() != null && data.GetExpectedType() == Unit.type)
        {
            data.SetCurrentExpectedTypeMet(true, Unit.type);
        }
        if (Unit.conversion == ConversionType.Any)
        {
            if (Unit.type == typeof(object)) return GenerateValue(Unit.value, data);
            return MakeSelectableForThisUnit($"({Unit.type.As().CSharpName(true, true)})") + GenerateValue(Unit.value, data);
        }
        else if (Unit.conversion == ConversionType.ToArrayOfObject)
        {
            return MakeSelectableForThisUnit("(") + $"{new ValueCode(GenerateValue(Unit.value, data), typeof(IEnumerable), ShouldCast(Unit.value, data, true), true)}" + MakeSelectableForThisUnit($").Cast<{"object".ConstructHighlight()}>().ToArray()");
        }
        else if (Unit.conversion == ConversionType.ToListOfObject)
        {
            return MakeSelectableForThisUnit("(") + $"{new ValueCode(GenerateValue(Unit.value, data), typeof(IEnumerable), ShouldCast(Unit.value, data, true), true)}" + MakeSelectableForThisUnit($").Cast<{"object".ConstructHighlight()}>().ToList()");
        }
        return base.GenerateValue(output, data);
    }
}
