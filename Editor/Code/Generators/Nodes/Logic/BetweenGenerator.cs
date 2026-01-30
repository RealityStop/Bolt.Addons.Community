using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Between))]
    public class BetweenGenerator : NodeGenerator<Between>
    {
        public BetweenGenerator(Unit unit) : base(unit) { }

        public override IEnumerable<string> GetNamespaces()
        {
            yield return "Unity.VisualScripting.Community";
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.CallCSharpUtilityMethod("IsWithin",
            writer.Action(w => GenerateValue(Unit.input, data, w)),
            writer.Action(w => GenerateValue(Unit.min, data, w)),
            writer.Action(w => GenerateValue(Unit.max, data, w)));
        }
    }
}
