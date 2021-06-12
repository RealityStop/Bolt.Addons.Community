using Unity.VisualScripting;
using UnityEngine;

namespace Bolt.Addons.Community.Fundamentals.Units.Collections.Editor
{
    public abstract class MachineVariableUnitDescriptor : UnitDescriptor<MachineVariableUnit>
    {
        public MachineVariableUnitDescriptor(MachineVariableUnit unit) : base(unit)
        {

        }

        protected abstract string Prefix { get; }

        protected override string DefinedTitle()
        {
            return $"{Prefix} Machine Variable";
        }

        protected override string DefinedShortTitle()
        {
            return $"{Prefix} Machine Variable";
        }

        protected override string DefaultTitle()
        {
            return $"{Prefix} Machine Variable";
        }

        protected override string DefaultShortTitle()
        {
            return $"{Prefix} Machine Variable";
        }
    }
}
