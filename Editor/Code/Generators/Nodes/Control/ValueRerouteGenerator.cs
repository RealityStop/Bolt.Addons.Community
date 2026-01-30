namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(ValueReroute))]
    public class ValueRerouteGenerator : NodeGenerator<ValueReroute>
    {
        public ValueRerouteGenerator(Unit unit) : base(unit)
        {
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            GenerateConnectedValue(Unit.input, data, writer);
        }
    }
}