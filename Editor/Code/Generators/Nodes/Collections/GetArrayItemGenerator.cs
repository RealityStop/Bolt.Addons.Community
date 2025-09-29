using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(GetArrayItem))]
    public class GetArrayItemGenerator : NodeGenerator<GetArrayItem>
    {
        public GetArrayItemGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            string arrayName = GenerateValue(Unit.array, data);
            string index = GenerateIndex(Unit.indexes, data);

            return arrayName + MakeClickableForThisUnit("[") + index + MakeClickableForThisUnit("]");
        }

        /// <summary>
        /// Generates the index part of the syntax, e.g., [2] or [i, j].
        /// </summary>
        private string GenerateIndex(List<ValueInput> indexes, ControlGenerationData data)
        {
            var indexStrings = new List<string>();

            foreach (var index in indexes)
            {
                indexStrings.Add(GenerateValue(index, data));
            }

            return string.Join(MakeClickableForThisUnit(", "), indexStrings);
        }
    }
}