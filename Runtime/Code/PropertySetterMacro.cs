using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.Code.PropertySetterMacro")]
    [TypeIcon(typeof(Property))]
    public sealed class PropertySetterMacro : Macro<FlowGraph>
    {
        public CodeAsset parentAsset;
#if UNITY_EDITOR
        public bool opened;
#endif

        public override FlowGraph DefaultGraph()
        {
            return new FlowGraph();
        }
    }
}
