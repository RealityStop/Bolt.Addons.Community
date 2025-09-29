using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(FirstItem))]
    public class FirstItemGenerator : NodeGenerator<FirstItem>
    {
        public FirstItemGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (typeof(IList).IsAssignableFrom(data.GetExpectedType()))
            {
                return GenerateValue(Unit.collection, data) + MakeClickableForThisUnit($"[{"0".NumericHighlight()}]");
            }
            else
            {
                NameSpaces = "System.Linq";
                return GenerateValue(Unit.collection, data) + MakeClickableForThisUnit($".Cast<{"object".ConstructHighlight()}>().First()");
            }
        }
    }
}
