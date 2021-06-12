using Unity.VisualScripting;

namespace Bolt.Addons.Community.Fundamentals.Units.Collections.Editor
{
    [Descriptor(typeof(SetMachineVariableUnit))]
    public sealed class SetMachineVariableUnitDescriptor : MachineVariableUnitDescriptor
    {
        public SetMachineVariableUnitDescriptor(SetMachineVariableUnit unit) : base(unit)
        {

        }
        protected override string Prefix => "Set";
    }
}
