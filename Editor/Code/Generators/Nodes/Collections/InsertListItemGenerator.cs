using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(InsertListItem))]
    public sealed class InsertListItemGenerator : NodeGenerator<InsertListItem>
    {
        public InsertListItemGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;
            List<string> t = new List<string>();
            data.SetExpectedType(Unit.listInput.type);
            var listCode = GenerateValue(Unit.listInput, data);
            data.RemoveExpectedType();
            var sourceType = GetSourceType(Unit.listInput, data);
            var isGenericList = sourceType.IsGenericType && sourceType.GetGenericTypeDefinition() == typeof(List<>);
            if (isGenericList)
                data.SetExpectedType(sourceType.GetGenericArguments()[0]);

            var itemCode = GenerateValue(Unit.item, data);

            if (isGenericList)
                data.RemoveExpectedType();

            output += CodeBuilder.Indent(indent) + listCode + MakeClickableForThisUnit($".Insert(") + GenerateValue(Unit.index, data) + MakeClickableForThisUnit(", ") + itemCode + MakeClickableForThisUnit(");") + "\n";
            output += GetNextUnit(Unit.exit, data, indent);
            return output;
        }
    }
}