using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

#if VISUAL_SCRIPTING_1_7
using SMachine = Unity.VisualScripting.ScriptMachine;
#else
using SMachine = Unity.VisualScripting.FlowMachine;
#endif

namespace Bolt.Addons.Community.Fundamentals
{
    [UnitTitle("Get Machine")]
    [TypeIcon(typeof(SMachine))]
    [UnitCategory("Community/Graphs")]
    public sealed class GetMachineUnit : Unit
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

    [UnitTitle("Get Machines")]
    [TypeIcon(typeof(SMachine))]
    [UnitCategory("Community/Graphs")]
    public sealed class GetMachinesUnit : Unit
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
        public ValueOutput machines;

        protected override void Definition()
        {
            target = ValueInput<GameObject>("target", (GameObject)null);
            target.NullMeansSelf();
            asset = ValueInput<ScriptGraphAsset>("asset", (ScriptGraphAsset)null);
            machines = ValueOutput<SMachine[]>("machine", (flow) =>
            {
                var machines = flow.GetValue<GameObject>(target).GetComponents<SMachine>();
                var _machines = new List<SMachine>();

                for (int i = 0; i < machines.Length; i++)
                {
                    if (machines[i].nest.macro == flow.GetValue<ScriptGraphAsset>(asset)) _machines.Add(machines[i]);
                }

                return _machines.ToArrayPooled();
            });
        }
    }
}