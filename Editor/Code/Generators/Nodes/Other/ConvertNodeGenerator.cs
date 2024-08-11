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
        NameSpace = "System.Linq";
    }

    public override string GenerateValue(ValueOutput output, ControlGenerationData data)
    {
        if (Unit.conversion == ConversionType.Any)
        {
            if(Unit.type == typeof(object)) return GenerateValue(Unit.value, data);
            return $"({Unit.type.As().CSharpName(true, true)})" + GenerateValue(Unit.value, data);
        }
        else if (Unit.conversion == ConversionType.ToArrayOfObject)
        {
            return $"({new ValueCode(GenerateValue(Unit.value, data), typeof(IEnumerable), ShouldCast(Unit.value, true), true)}).Cast<{"object".ConstructHighlight()}>().ToArray()";
        }
        else if (Unit.conversion == ConversionType.ToListOfObject)
        {
            return $"({new ValueCode(GenerateValue(Unit.value, data), typeof(IEnumerable), ShouldCast(Unit.value, true), true)}).Cast<{"object".ConstructHighlight()}>().ToList()";
        }
        return base.GenerateValue(output, data);
    }
}
