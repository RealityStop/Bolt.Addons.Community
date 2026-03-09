using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(FirstItem))]
    public class FirstItemGenerator : NodeGenerator<FirstItem>
    {
        public FirstItemGenerator(Unit unit) : base(unit) { }

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
                    w.Int(0);
                });
            }
            else
            {
                GenerateValue(Unit.collection, data, writer);
                writer.Write(".Cast<").Write("object".ConstructHighlight()).Write(">().First()");
            }
        }
    }
}
