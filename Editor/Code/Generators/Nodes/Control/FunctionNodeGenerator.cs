using Unity.VisualScripting.Community.Libraries.CSharp;
namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(FunctionNode))]
    public sealed class FunctionNodeGenerator : NodeGenerator<FunctionNode>
    {
        public FunctionNodeGenerator(FunctionNode unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.invoke.connection?.destination;
            if (!Unit.invoke.hasAnyConnection) return "\n";
            return NodeGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit).GenerateControl(destination, data, indent);
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeClickableForThisUnit(output.key.LegalMemberName().VariableHighlight());
        }
    }
}