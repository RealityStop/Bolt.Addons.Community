
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ClearDictionary))]
    public class ClearDictionaryGenerator : NodeGenerator<ClearDictionary>
    {
        public ClearDictionaryGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return base.GenerateValue(output, data);
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            data.SetExpectedType(typeof(System.Collections.IDictionary));
            string output = CodeBuilder.Indent(indent) + GenerateValue(Unit.dictionaryInput, data) + MakeClickableForThisUnit(".Clear(");
            data.RemoveExpectedType();
            output = output + MakeClickableForThisUnit(");") + "\n" + GetNextUnit(Unit.exit, data, indent);
            return output;
        }
    }
}
