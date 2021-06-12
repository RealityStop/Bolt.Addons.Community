using Unity.VisualScripting;

namespace Bolt.Addons.Community.Fundamentals.Units.Collections.Editor
{
    [Descriptor(typeof(GetMachineVariableUnit))]
    public sealed class GetMachineVariableUnitDescriptor : MachineVariableUnitDescriptor
    {
        public GetMachineVariableUnitDescriptor(GetMachineVariableUnit unit) : base(unit)
        {

        }

        protected override string Prefix => "Get";
    }
}
