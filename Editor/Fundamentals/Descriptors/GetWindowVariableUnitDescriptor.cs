using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility.Editor
{
    [Descriptor(typeof(GetWindowVariableUnit))]
    public sealed class GetWindowVariableUnitDescriptor : WindowVariableUnitDescriptor
    {
        public GetWindowVariableUnitDescriptor(GetWindowVariableUnit unit) : base(unit)
        {

        }
        protected override string Prefix => "Get";
    }
}
