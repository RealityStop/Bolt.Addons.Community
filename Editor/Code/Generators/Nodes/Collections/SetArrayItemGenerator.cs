using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(SetArrayItem))]
    public class SetArrayItemGenerator : NodeGenerator<SetArrayItem>
    {
        public SetArrayItemGenerator(Unit unit) : base(unit) { }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            string arrayName = GenerateValue(Unit.array, data);
            string indexExpression = GenerateIndex(Unit.indexes, data);
            string valueExpression = GenerateValue(Unit.value, data);

            var indexString = MakeClickableForThisUnit("[") + indexExpression + MakeClickableForThisUnit("]");
            string assignment = $"{arrayName}{indexString}{MakeClickableForThisUnit(" = ")}{valueExpression}{MakeClickableForThisUnit(";")}";

            return assignment;
        }

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
