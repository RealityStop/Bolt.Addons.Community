#if VISUAL_SCRIPTING_1_7
namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(HasScriptGraph))]
    public class HasScriptGraphGenerator : HasGraphGenerator<FlowGraph, ScriptGraphAsset, ScriptMachine>
    {
        public HasScriptGraphGenerator(Unit unit) : base(unit) { }
    }
}
#endif