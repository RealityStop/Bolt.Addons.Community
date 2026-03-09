namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(FlowReroute))]
    public class FlowRerouteGenerator : NodeGenerator<FlowReroute>
    {
        public FlowRerouteGenerator(Unit unit) : base(unit)
        {
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            GenerateExitControl(Unit.output, data, writer);
        }
    }
}