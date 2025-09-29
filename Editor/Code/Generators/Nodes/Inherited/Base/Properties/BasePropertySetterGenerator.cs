using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(BasePropertySetterUnit))]
    public class BasePropertySetterGenerator : NodeGenerator<BasePropertySetterUnit>
    {
        public BasePropertySetterGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit(string.Concat("base".ConstructHighlight(), ".", Unit.member.name, " = ")) + GenerateValue(Unit.value, data) + MakeClickableForThisUnit(";") + "\n";
            output += GetNextUnit(Unit.exit, data, indent);
            return output;
        }
    }
}
