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
            output += CodeBuilder.Indent(indent) + GenerateValue(Unit.listInput, data) + MakeClickableForThisUnit($".Insert(") + GenerateValue(Unit.index, data) + MakeClickableForThisUnit(", ") + GenerateValue(Unit.item, data) + MakeClickableForThisUnit(");") + "\n";
            output += GetNextUnit(Unit.exit, data, indent);
            return output;
        }
    } 
}