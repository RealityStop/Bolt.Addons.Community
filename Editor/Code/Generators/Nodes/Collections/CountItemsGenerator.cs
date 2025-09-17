using Unity.VisualScripting;
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(CountItems))]
    public class CountItemsGenerator : NodeGenerator<CountItems>
    {
        public CountItemsGenerator(Unit unit) : base(unit)
        {
            NameSpaces = "System.Collections";
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            var type = GetSourceType(Unit.collection, data);
            data.SetExpectedType(typeof(ICollection));
            if (type != null && type.IsArray)
            {
                var code = GenerateValue(Unit.collection, data) + MakeClickableForThisUnit("." + "Length".VariableHighlight());
                data.RemoveExpectedType();
                return code;
            }
            var _code = GenerateValue(Unit.collection, data) + MakeClickableForThisUnit("." + "Count".VariableHighlight());
            data.RemoveExpectedType();
            return _code;
        }
    }
}