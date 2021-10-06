using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.Code.ConstructorMacro")]
    public sealed class ConstructorMacro : Macro<FlowGraph>
    {
        public override FlowGraph DefaultGraph()
        {
            return new FlowGraph();
        }
    }
}
