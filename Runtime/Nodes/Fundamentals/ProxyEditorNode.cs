using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [UnitCategory("Community/Editor")]
    [RenamedFrom("Bolt.Addons.Community.ProxyEditorUnit")]
    public abstract class ProxyEditorNode : Unit
    {
        [DoNotSerialize]
        public ValueInput fauxInput;

        protected override void Definition()
        {
            
        }
    }
}