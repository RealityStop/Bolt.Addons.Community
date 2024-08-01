using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.Code.PropertyGetterMacro")]
    [TypeIcon(typeof(Property))]
    public sealed class PropertyGetterMacro : Macro<FlowGraph>
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
    
    /// <summary>
    /// This is a empty class used for the typeIcon
    /// </summary>
    public class Property { }
}
