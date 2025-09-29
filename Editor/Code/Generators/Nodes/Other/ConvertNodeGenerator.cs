using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community 
{
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
                NameSpaces = "";
                if (Unit.type == typeof(object)) return GenerateValue(Unit.value, data);
                NameSpaces = Unit.type.Namespace;
                return Unit.CreateClickableString().Ignore(GenerateValue(Unit.value, data)).Cast(Unit.type, true);
            }
            else if (Unit.conversion == ConversionType.ToArrayOfObject)
            {
                NameSpaces = "System.Linq";
                return Unit.CreateClickableString().Ignore(GenerateValue(Unit.value, data)).Cast(typeof(IEnumerable), ShouldCast(Unit.value, data, true)).Dot().Clickable($"Cast<{"object".ConstructHighlight()}>().ToArray()");
            }
            else if (Unit.conversion == ConversionType.ToListOfObject)
            {
                NameSpaces = "System.Linq";
                return Unit.CreateClickableString().Ignore(GenerateValue(Unit.value, data)).Cast(typeof(IEnumerable), ShouldCast(Unit.value, data, true)).Dot().Clickable($"Cast<{"object".ConstructHighlight()}>().ToList()");
            }
            return base.GenerateValue(output, data);
        }
    } 
}
