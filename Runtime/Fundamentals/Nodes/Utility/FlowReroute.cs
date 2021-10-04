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

        protected override void Definition()
        {
            input = ControlInput("in", (flow) => { return output; });
            output = ControlOutput("out");
            Succession(input, output);
        }
    }
}