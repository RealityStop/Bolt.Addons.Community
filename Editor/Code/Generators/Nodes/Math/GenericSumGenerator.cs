using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using System;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(GenericSum))]
    public class GenericSumGenerator : NodeGenerator<GenericSum>
    {
        public GenericSumGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateValue(ValueOutput output)
        {
            List<string> values = new List<string>();
            
            foreach (var item in this.Unit.multiInputs)
            {
                values.Add(base.GenerateValue(item));
            }
            
            return string.Join(" + ", values);
        }
    }
}