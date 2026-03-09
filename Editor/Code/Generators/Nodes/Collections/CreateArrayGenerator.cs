using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.Humility;
using System.Linq;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(CreateArray))]
    public class CreateArrayGenerator : NodeGenerator<CreateArray>
    {
        public CreateArrayGenerator(Unit unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (data.GetExpectedType() != null && !data.IsCurrentExpectedTypeMet())
            {
                if (data.GetExpectedType().IsStrictlyAssignableFrom(Unit.type))
                {
                    data.MarkExpectedTypeMet(Unit.type);
                }
            }

            CodeWriter.DimensionParameter[] dimensions = new CodeWriter.DimensionParameter[Unit.dimensions];
            var index = 0;
            foreach (var valueInput in Unit.indexes)
            {
                dimensions[index] = writer.Action(w => GenerateValue(valueInput, data, w));
                index++;
            }

            writer.NewArray(Unit.type, dimensions);
        }
    }
}