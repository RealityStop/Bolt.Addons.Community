using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.Code.PropertySetterMacro")]
    [TypeIcon(typeof(Property))]
    public sealed class PropertySetterMacro : Macro<FlowGraph>
    {
        [Inspectable]
        public ClassAsset classAsset;

        [Inspectable]
        public StructAsset structAsset;
#if UNITY_EDITOR
        public bool opened;
#endif

        public override FlowGraph DefaultGraph()
        {
            return new FlowGraph();
        }
    }
}
