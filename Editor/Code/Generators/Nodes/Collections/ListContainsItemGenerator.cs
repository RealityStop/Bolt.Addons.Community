using System;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ListContainsItem))]
    public class ListContainsItemGenerator : NodeGenerator<ListContainsItem>
    {
        public ListContainsItemGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return base.GenerateValue(this.Unit.list, data) + MakeClickableForThisUnit(".Contains(") + base.GenerateValue(this.Unit.item, data) + MakeClickableForThisUnit(")");
        }
    }
}