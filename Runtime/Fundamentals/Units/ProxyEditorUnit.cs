using Unity.VisualScripting;

namespace Bolt.Addons.Community.Fundamentals
{
    [UnitCategory("Community/Editor")]
    public abstract class ProxyEditorUnit : Unit
    {
        [DoNotSerialize]
        public ValueInput fauxInput;

        protected override void Definition()
        {
            
        }
    }
}