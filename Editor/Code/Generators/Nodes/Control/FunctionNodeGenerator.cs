using Unity.VisualScripting.Community.Libraries.CSharp;
namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(FunctionNode))]
    public sealed class FunctionNodeGenerator : NodeGenerator<FunctionNode>
    {
        public FunctionNodeGenerator(FunctionNode unit) : base(unit)
        {
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (Unit.invoke == null || !Unit.invoke.hasAnyConnection)
            {
                return;
            }

            GenerateChildControl(Unit.invoke, data, writer);
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.Write(output.key.LegalMemberName().VariableHighlight());
        }
    }
}