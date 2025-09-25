namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(GetScriptGraphs))]
    public class GetScriptGraphsGenerator : GetGraphsGenerator<FlowGraph, ScriptGraphAsset, ScriptMachine>
    {
        public GetScriptGraphsGenerator(Unit unit) : base(unit) { }
    }
}