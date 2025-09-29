using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community 
{
    [NodeGenerator(typeof(RemoveListItem))]
    public sealed class RemoveListItemGenerator : NodeGenerator<RemoveListItem>
    {
        public RemoveListItemGenerator(Unit unit) : base(unit)
        {
        }
    
        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;
            data.SetExpectedType(Unit.listInput.type);
            var listCode = GenerateValue(Unit.listInput, data);
            data.RemoveExpectedType();
            output += CodeBuilder.Indent(indent) + listCode + MakeClickableForThisUnit($".Remove(") + GenerateValue(Unit.item, data) + MakeClickableForThisUnit(");") + "\n";
            output += GetNextUnit(Unit.exit, data, indent);
            return output;
        }
    } 
}