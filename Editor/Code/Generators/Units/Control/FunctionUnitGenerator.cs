namespace Unity.VisualScripting.Community
{
    [UnitGenerator(typeof(FunctionNode))]
    public sealed class FunctionUnitGenerator : UnitGenerator<FunctionNode>
    {
        public FunctionUnitGenerator(FunctionNode unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.invoke.connection?.destination;
            if (!Unit.invoke.hasAnyConnection) return "\n";
            return UnitGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit).GenerateControl(destination, data, indent);
        }
    }
}