namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(GetMachinesNode))]
    public class GetMachineNodesGenerator : NodeGenerator<GetMachinesNode>
    {
        public GetMachineNodesGenerator(Unit unit) : base(unit) { }
        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.CallCSharpUtilityMethod("GetScriptMachines", 
            writer.Action(() => GenerateValue(Unit.target, data, writer)), 
            writer.Action(() => GenerateValue(Unit.asset, data, writer)));
        }
    }
}