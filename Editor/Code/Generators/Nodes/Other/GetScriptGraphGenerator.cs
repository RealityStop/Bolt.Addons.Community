#if VISUAL_SCRIPTING_1_7
namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(GetScriptGraph))]
    public class GetScriptGraphGenerator : GetGraphGenerator<FlowGraph, ScriptGraphAsset, ScriptMachine>
    {
        public GetScriptGraphGenerator(Unit unit) : base(unit) { }
    }
}
#endif