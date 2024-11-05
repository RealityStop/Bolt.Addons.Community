using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    
    [NodeGenerator(typeof(ValueReroute))]
    public class ValueRerouteGenerator : NodeGenerator<ValueReroute>
    {
        public ValueRerouteGenerator(Unit unit) : base(unit)
        {
        }
    
                public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            
            return GenerateValue(Unit.input, data);
        }
    }
    
}