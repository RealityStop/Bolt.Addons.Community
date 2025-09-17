using Unity.VisualScripting;
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(AddListItem))]
    public class AddListItemGenerator : NodeGenerator<AddListItem>
    {
        public AddListItemGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            data.SetExpectedType(Unit.listInput.type);
            var listCode = GenerateValue(Unit.listInput, data);
            data.RemoveExpectedType();
            return CodeBuilder.Indent(indent) + listCode + MakeClickableForThisUnit(".Add(", true) + base.GenerateValue(this.Unit.item, data) + MakeClickableForThisUnit(");", true) + "\n" + this.GetNextUnit(this.Unit.exit, data, indent);
        }
    }
}