using UnityEngine;
namespace Unity.VisualScripting.Community
{
    [UnitCategory("Community/Utility")]
    [RenamedFrom("Lasm.BoltExtensions.FlowReroute")]
    [RenamedFrom("Lasm.UAlive.FlowReroute")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.Units.Utility.FlowReroute")]
    public sealed class FlowReroute : Unit
    {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput input;
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput output;

        [Inspectable]
        public Color InputColor = Color.white;

        [Inspectable]
        public Color OutputColor = Color.white;

        [Inspectable]
        public bool SnapToGrid;

        [Inspectable]
        public bool InputVisible = true;
        [Inspectable]
        public bool OutputVisible = true;

        [Inspectable]
        public bool showFlowOnHover = true;

        protected override void Definition()
        {
            input = ControlInput("in", (flow) => { return output; });
            output = ControlOutput("out");
            Succession(input, output);
        }
    }
}