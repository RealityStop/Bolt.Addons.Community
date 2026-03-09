using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(AsUnit))]
    public class AsUnitGenerator : NodeGenerator<AsUnit>
    {
        public AsUnitGenerator(Unit unit) : base(unit)
        {
        }
    
        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (data.GetExpectedType() != null && data.GetExpectedType() == Unit.AsType)
            {
                data.MarkExpectedTypeMet(Unit.AsType);
            }
            writer.Write("(");
            GenerateValue(Unit.Value, data, writer);
            writer.Write(" " + "as".ConstructHighlight() + " " + Unit.AsType.As().CSharpName(false, true, true).TypeHighlight() + ")");
        }
    }
}
