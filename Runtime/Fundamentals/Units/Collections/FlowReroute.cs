using Bolt;
using Ludiq;


namespace Bolt.Addons.Community.Fundamentals.Units.Utility
{
    [UnitCategory("Community/Utility")]
    [RenamedFrom("Lasm.BoltExtensions.FlowReroute")]
    [RenamedFrom("Lasm.UAlive.FlowReroute")]
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