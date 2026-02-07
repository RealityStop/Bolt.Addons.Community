using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(CreateList))]
    public class CreateListGenerator : NodeGenerator<CreateList>
    {
        public CreateListGenerator(Unit unit) : base(unit)
        {
        }

        public override IEnumerable<string> GetNamespaces()
        {
            yield return "Unity.VisualScripting";
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            var Type = typeof(AotList);
            var expectedType = data.GetExpectedType();
            if (expectedType != null && (expectedType.IsArray || (typeof(IList).IsAssignableFrom(expectedType) && expectedType.IsConcrete())))
            {
                Type = expectedType;
            }
            writer.Write("new ".ConstructHighlight() + writer.GetTypeNameHighlighted(Type) + (!Type.IsArray ? "()" : string.Empty)).Space().Braces(w =>
            {
                w.Space();
                for (int i = 0; i < Unit.multiInputs.Count; i++)
                {
                    if (i != 0) writer.ParameterSeparator();
                    GenerateValue(Unit.multiInputs[i], data, writer);
                }
                w.Space();
            });
        }
    }
}