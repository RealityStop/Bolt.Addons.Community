using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(CreateArray))]
    public class CreateArrayGenerator : NodeGenerator<CreateArray>
    {
        public CreateArrayGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (data.GetExpectedType() != null && !data.IsCurrentExpectedTypeMet())
            {
                data.SetCurrentExpectedTypeMet(true, Unit.type);

            }
            string typeName = Unit.type.As().CSharpName(false, true);
            string dimensionString = GenerateDimensions(Unit.dimensions, data);
            data.CreateSymbol(Unit, Unit.type);

            return MakeClickableForThisUnit("new ".ConstructHighlight() + $"{typeName}") + dimensionString;
        }

        /// <summary>
        /// Generates the array dimension part of the syntax, e.g., [10, 5, 2].
        /// </summary>
        private string GenerateDimensions(int dimensions, ControlGenerationData data)
        {
            var lengthInputs = new List<string>();

            for (int i = 0; i < dimensions; i++)
            {
                lengthInputs.Add(GenerateValue(Unit.indexes[i], data));
            }

            return MakeClickableForThisUnit("[") + $"{string.Join(MakeClickableForThisUnit(", "), lengthInputs)}" + MakeClickableForThisUnit("]");
        }
    }
}