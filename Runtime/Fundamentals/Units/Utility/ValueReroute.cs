using Bolt;
using Ludiq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bolt.Addons.Community.Fundamentals.Units.Utility
{
    [UnitCategory("Community/Utility")]
    [RenamedFrom("Lasm.BoltExtensions.ValueReroute")]
    [RenamedFrom("Lasm.UAlive.ValueReroute")]
    public sealed class ValueReroute : Unit
    {
        [DoNotSerialize][PortLabelHidden]
        public ValueInput input;
        [DoNotSerialize][PortLabelHidden]
        public ValueOutput output;
        [Serialize]
        public Type portType = typeof(object);

        protected override void Definition()
        {
            input = ValueInput(portType, "in");
            output = ValueOutput(portType, "out", (flow) => { return flow.GetValue(input); });
            Requirement(input, output);
        }
    }
}