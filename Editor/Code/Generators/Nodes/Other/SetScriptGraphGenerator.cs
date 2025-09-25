namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(SetScriptGraph))]
    public class SetScriptGraphGenerator : SetGraphGenerator<FlowGraph, ScriptGraphAsset, ScriptMachine>
    {
        public SetScriptGraphGenerator(Unit unit) : base(unit) { }
    }
}