using Bolt;
using Ludiq;
using Lasm.BoltAddons.VariableTags;

namespace Lasm.BoltAddons.VariableTags.Editor
{
    [Descriptor(typeof(VariableTagUnit))]
    public sealed class VariableTagDescriptor : UnitDescriptor<VariableTagUnit>
    {
        public VariableTagDescriptor(VariableTagUnit unit) : base(unit)
        {

        }
    }
}
