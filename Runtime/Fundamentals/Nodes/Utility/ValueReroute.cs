﻿using System;

namespace Unity.VisualScripting.Community
{
    [UnitCategory("Community/Utility")]
    [RenamedFrom("Lasm.BoltExtensions.ValueReroute")]
    [RenamedFrom("Lasm.UAlive.ValueReroute")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.Units.Utility.ValueReroute")]
    public sealed class ValueReroute : Unit
    {
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput input;
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput output;
        [Serialize]
        public Type portType = typeof(object);

        [Inspectable]
        public bool SnapToGrid;

        protected override void Definition()
        {
            input = ValueInput(portType, "in");
            output = ValueOutput(portType, "out", (flow) => { return flow.GetValue(input); });
            Requirement(input, output);
        }
    }
}