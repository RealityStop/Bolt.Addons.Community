using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Bolt.Addons.Community.Fundamentals
{
    [UnitTitle("Get Machine")]
    [TypeIcon(typeof(ScriptMachine))]
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
            machine = ValueOutput<ScriptMachine>("machine", (flow) =>
            {
                var machines = flow.GetValue<GameObject>(target).GetComponents<ScriptMachine>();
                ScriptMachine _machine = null;

                for (int i = 0; i < machines.Length; i++)
                {
                    if (machines[i].nest.macro == flow.GetValue<ScriptGraphAsset>(asset)) return machines[i];
                }

                return _machine;
            });
        }
    }

    [UnitTitle("Get Machines")]
    [TypeIcon(typeof(ScriptMachine))]
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
            machines = ValueOutput<ScriptMachine[]>("machine", (flow) =>
            {
                var machines = flow.GetValue<GameObject>(target).GetComponents<ScriptMachine>();
                var _machines = new List<ScriptMachine>();

                for (int i = 0; i < machines.Length; i++)
                {
                    if (machines[i].nest.macro == flow.GetValue<ScriptGraphAsset>(asset)) _machines.Add(machines[i]);
                }

                return _machines.ToArrayPooled();
            });
        }
    }
}