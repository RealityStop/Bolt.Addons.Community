using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("ResetSavedVariable")]
    [RenamedFrom("Unity.VisualScripting.Community.ResetSavedVariable")]
    [UnitCategory("Community/Variables")]
    [UnitTitle("Reset")]
    [UnitSurtitle("Saved Variables")]
    [TypeIcon(typeof(FlowGraph))]
    public class ResetSavedVariables : VariadicNode<string>
    {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput Reset;
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput OnReset;

        protected override void Definition()
        {
            base.Definition();
            Reset = ControlInput(nameof(Reset), Enter);
            OnReset = ControlOutput(nameof(OnReset));

            Succession(Reset, OnReset);
        }

        public ControlOutput Enter(Flow flow)
        {
            foreach (var arg in arguments)
            {
                string key = flow.GetValue<string>(arg);
                CSharpUtility.ResetSavedVariable(key);
            }
            return OnReset;
        }

        protected override void BuildRelations(ValueInput arg)
        {
            Requirement(arg, Reset);
        }
    }
}