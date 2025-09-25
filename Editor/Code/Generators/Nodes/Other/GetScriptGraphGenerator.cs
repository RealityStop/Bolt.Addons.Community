namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(GetScriptGraph))]
    public class GetScriptGraphGenerator : GetGraphGenerator<FlowGraph, ScriptGraphAsset, ScriptMachine>
    {
        public GetScriptGraphGenerator(Unit unit) : base(unit) { }
    }
}