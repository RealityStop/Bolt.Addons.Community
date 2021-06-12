using Unity.VisualScripting;

namespace Bolt.Addons.Community.Fundamentals
{
    [UnitTitle("Machine Is")]
    [TypeIcon(typeof(ScriptMachine))]
    [UnitCategory("Community/Graphs")]
    public sealed class MachineIsUnit : Unit
    {
        [DoNotSerialize]
        [NullMeansSelf]
        [PortLabelHidden]
        public ValueInput target;
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput asset;
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput result;

        protected override void Definition()
        {
            target = ValueInput<ScriptMachine>("target", (ScriptMachine)null);
            target.NullMeansSelf();
            asset = ValueInput<ScriptGraphAsset>("asset", (ScriptGraphAsset)null);
            result = ValueOutput<bool>("machine", (flow) =>
            {
                var macro = flow.GetValue<ScriptMachine>(target)?.nest.macro;
                if (macro != null) return macro == flow.GetValue<ScriptGraphAsset>(asset);
                return false;
            });
        }
    }
}