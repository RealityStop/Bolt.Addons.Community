
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(SetDictionaryItem))]
    public class SetDictionaryItemGenerator : NodeGenerator<SetDictionaryItem>
    {
        public SetDictionaryItemGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return base.GenerateValue(output, data);
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return string.Concat(base.GenerateValue(this.Unit.dictionary, data), MakeSelectableForThisUnit("[", true), base.GenerateValue(this.Unit.key, data), MakeSelectableForThisUnit("] = ", true)) + base.GenerateValue(this.Unit.value, data) + MakeSelectableForThisUnit(";", true) + "\n" + GetNextUnit(this.Unit.exit, data, indent);
        }
    }
}
