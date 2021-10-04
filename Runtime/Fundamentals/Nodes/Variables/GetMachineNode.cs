using UnityEngine;

#if VISUAL_SCRIPTING_1_7
using SMachine = Unity.VisualScripting.ScriptMachine;
#else
using SMachine = Unity.VisualScripting.FlowMachine;
#endif

namespace Unity.VisualScripting.Community
{
    [UnitTitle("Get Machine")]
    [TypeIcon(typeof(SMachine))]
    [UnitCategory("Community/Graphs")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.GetMachineUnit")]
    public sealed class GetMachineNode : Unit
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
        public ValueOutput machine;

        protected override void Definition()
        {
            target = ValueInput<GameObject>("target", (GameObject)null);
            target.NullMeansSelf();
            asset = ValueInput<ScriptGraphAsset>("asset", (ScriptGraphAsset)null);
            machine = ValueOutput<SMachine>("machine", (flow) =>
            {
                var machines = flow.GetValue<GameObject>(target).GetComponents<SMachine>();
                SMachine _machine = null;

                for (int i = 0; i < machines.Length; i++)
                {
                    if (machines[i].nest.macro == flow.GetValue<ScriptGraphAsset>(asset)) return machines[i];
                }

                return _machine;
            });
        }
    }
}