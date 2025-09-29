using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community 
{
    [NodeGenerator(typeof(AsUnit))]
    public class AsUnitGenerator : NodeGenerator<AsUnit>
    {
        public AsUnitGenerator(Unit unit) : base(unit)
        {
        }
    
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (data.GetExpectedType() != null && data.GetExpectedType() == Unit.AsType)
            {
                data.SetCurrentExpectedTypeMet(true, Unit.AsType);
            }
            return MakeClickableForThisUnit("(") + GenerateValue(Unit.Value, data) + MakeClickableForThisUnit(" as ".ConstructHighlight() + Unit.AsType.As().CSharpName(false, true, true) + ")");
        }
    } 
}
