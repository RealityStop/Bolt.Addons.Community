using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(FlowReroute))]
    public class FlowRerouteGenerator : NodeGenerator<FlowReroute>
    {
        public FlowRerouteGenerator(Unit unit) : base(unit)
        {
        }
    
        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return GetNextUnit(Unit.output, data, indent);
        }
    }
    
}