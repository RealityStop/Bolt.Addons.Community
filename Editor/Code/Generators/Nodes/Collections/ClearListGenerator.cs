using Unity.VisualScripting;
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ClearList))]
    public class ClearListGenerator : NodeGenerator<ClearList>
    {
        public ClearListGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return GenerateValue(Unit.listInput, data);
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return CodeBuilder.Indent(indent) + GenerateValue(Unit.listInput, data) + MakeClickableForThisUnit(".Clear();") + "\n" + GetNextUnit(Unit.exit, data, indent);
        }
    }
}