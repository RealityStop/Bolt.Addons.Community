using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(LastItem))]
    public class LastItemGenerator : NodeGenerator<LastItem>
    {
        public LastItemGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (typeof(IList).IsAssignableFrom(data.GetExpectedType()))
            {
                var listCode = GenerateValue(Unit.collection, data);
                return listCode + MakeClickableForThisUnit($"[") + listCode + MakeClickableForThisUnit($".Count - {"1".NumericHighlight()}]");
            }
            else
            {
                NameSpaces = "System.Linq";
                return GenerateValue(Unit.collection, data) + MakeClickableForThisUnit($".Cast<{"object".ConstructHighlight()}>().Last()");
            }
        }
    }
}
