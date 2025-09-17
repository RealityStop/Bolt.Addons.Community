
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(DictionaryContainsKey))]
    public class DictionaryContainsKeyGenerator : NodeGenerator<DictionaryContainsKey>
    {
        public DictionaryContainsKeyGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return GenerateValue(Unit.dictionary, data) + MakeClickableForThisUnit(".Contains(") + GenerateValue(Unit.key, data) + MakeClickableForThisUnit(")");
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }
    }
}
