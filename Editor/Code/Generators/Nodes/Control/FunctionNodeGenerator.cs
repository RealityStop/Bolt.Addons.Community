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
            if (!Unit.invoke.hasAnyConnection) return "\n";
            return GetNextUnit(Unit.invoke, data, indent);
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeClickableForThisUnit(output.key.LegalMemberName().VariableHighlight());
        }
    }
}