using Unity.VisualScripting;

namespace Bolt.Addons.Community.Code
{
    public sealed class PropertyGetterMacro : Macro<FlowGraph>
    {
#if UNITY_EDITOR
        public bool opened;
#endif

        public override FlowGraph DefaultGraph()
        {
            return new FlowGraph();
        }
    }
}
