using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(BasePropertyGetterUnit))]
    public class BasePropertyGetterGenerator : NodeGenerator<BasePropertyGetterUnit>
    {
        public BasePropertyGetterGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeClickableForThisUnit(string.Concat("base".ConstructHighlight(), ".", this.Unit.member.name));
        }
    }
}
