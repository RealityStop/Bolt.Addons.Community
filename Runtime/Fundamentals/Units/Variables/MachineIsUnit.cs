﻿using Unity.VisualScripting;

#if VISUAL_SCRIPTING_1_7
using SMachine = Unity.VisualScripting.ScriptMachine;
#else
using SMachine = Unity.VisualScripting.FlowMachine;
#endif

namespace Bolt.Addons.Community.Fundamentals
{
    [UnitTitle("Machine Is")]
    [TypeIcon(typeof(SMachine))]
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
            target = ValueInput<SMachine>("target", (SMachine)null);
            target.NullMeansSelf();
            asset = ValueInput<ScriptGraphAsset>("asset", (ScriptGraphAsset)null);
            result = ValueOutput<bool>("machine", (flow) =>
            {
                var macro = flow.GetValue<SMachine>(target)?.nest.macro;
                if (macro != null) return macro == flow.GetValue<ScriptGraphAsset>(asset);
                return false;
            });
        }
    }
}