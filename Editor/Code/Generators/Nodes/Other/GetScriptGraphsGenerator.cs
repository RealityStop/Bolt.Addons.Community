#if VISUAL_SCRIPTING_1_7
namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(GetScriptGraphs))]
    public class GetScriptGraphsGenerator : GetGraphsGenerator<FlowGraph, ScriptGraphAsset, ScriptMachine>
    {
        public GetScriptGraphsGenerator(Unit unit) : base(unit) { }
    }
}
#endif