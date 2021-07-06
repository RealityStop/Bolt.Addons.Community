using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility.Editor
{
    [Descriptor(typeof(SetWindowVariableUnit))]
    public sealed class SetWindowVariableUnitDescriptor : WindowVariableUnitDescriptor
    {
        public SetWindowVariableUnitDescriptor(SetWindowVariableUnit unit) : base(unit)
        {

        }
        protected override string Prefix => "Set";
    }
}
