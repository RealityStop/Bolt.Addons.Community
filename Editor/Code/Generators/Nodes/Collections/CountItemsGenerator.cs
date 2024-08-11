using Unity.VisualScripting;
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(CountItems))]
    public class CountItemsGenerator : NodeGenerator<CountItems>
    {
        public CountItemsGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            var type = GetSourceType(Unit.collection, data);
            if(type != null && type.IsArray)
            {
                return GenerateValue(Unit.collection, data) + ".Length";
            }
            return GenerateValue(Unit.collection, data) + ".Count";
        }
    }
}