namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(HasScriptGraph))]
    public class HasScriptGraphGenerator : HasGraphGenerator<FlowGraph, ScriptGraphAsset, ScriptMachine>
    {
        public HasScriptGraphGenerator(Unit unit) : base(unit) { }
    }
}