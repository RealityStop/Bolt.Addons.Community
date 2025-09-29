using Unity.VisualScripting;
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(RemoveListItemAt))]
    public class RemoveListItemAtGenerator : NodeGenerator<RemoveListItemAt>
    {
        public RemoveListItemAtGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            data.SetExpectedType(Unit.listInput.type);
            var listCode = GenerateValue(Unit.listInput, data);
            data.RemoveExpectedType();
            return CodeBuilder.Indent(indent) + listCode + MakeClickableForThisUnit(".RemoveAt(", true) + base.GenerateValue(this.Unit.index, data) + MakeClickableForThisUnit(");", true) + "\n" + GetNextUnit(this.Unit.exit, data, indent);
        }
    }
}