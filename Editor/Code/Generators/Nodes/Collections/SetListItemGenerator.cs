using Unity.VisualScripting;
using System;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(SetListItem))]
    public class SetListItemGenerator : NodeGenerator<SetListItem>
    {
        public SetListItemGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return string.Concat(base.GenerateValue(this.Unit.list, data), MakeSelectableForThisUnit("[", true), base.GenerateValue(this.Unit.index, data), MakeSelectableForThisUnit("] = ", true)) + base.GenerateValue(this.Unit.item, data) + MakeSelectableForThisUnit(";", true) + "\n" + GetNextUnit(this.Unit.exit, data, indent);
        }
    }
}