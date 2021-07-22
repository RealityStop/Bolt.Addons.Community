using Unity.VisualScripting;

namespace Bolt.Addons.Community.Code
{
    public sealed class ConstructorMacro : Macro<FlowGraph>
    {
        public override FlowGraph DefaultGraph()
        {
            return new FlowGraph();
        }
    }
}
