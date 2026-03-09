using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(LastItem))]
    public class LastItemGenerator : NodeGenerator<LastItem>
    {
        public LastItemGenerator(Unit unit) : base(unit) { }

        public override IEnumerable<string> GetNamespaces()
        {
            yield return "System.Linq";
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (typeof(IList).IsAssignableFrom(data.GetExpectedType()))
            {
                GenerateValue(Unit.collection, data, writer);
                writer.Brackets(w =>
                {
                    w.GetMember(writer.Action(w1 => GenerateValue(Unit.collection, data, w1)), "Count");
                    w.Subtract().Int(1);
                });
            }
            else
            {
                GenerateValue(Unit.collection, data, writer);
                writer.Write(".Cast<").Write("object".ConstructHighlight()).Write(">().Last()");
            }
        }
    }
}
