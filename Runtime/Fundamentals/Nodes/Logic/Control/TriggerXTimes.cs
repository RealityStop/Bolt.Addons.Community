using UnityEngine;
using Unity.VisualScripting;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.TriggerXTimes")]
    [UnitTitle("LimitedTrigger")]
    [UnitCategory("Community\\Control")]
    [TypeIcon(typeof(Once))]
    public class TriggerXTimes : Unit
    {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput Input;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput Exit;

        [DoNotSerialize]
        public ControlOutput After;

        [DoNotSerialize]
        public ControlInput Reset;

        [DoNotSerialize]
        public ValueInput Times;

        private int timesTriggered = 0;

        protected override void Definition()
        {
            Input = ControlInput(nameof(Input), IncreaseTimes);
            Reset = ControlInput(nameof(Reset), ResetTimes);
            Exit = ControlOutput(nameof(Exit));
            After = ControlOutput(nameof(After));
            Times = ValueInput<int>(nameof(Times), 1);

            Succession(Reset, Exit);
            Succession(Input, Exit);
            Succession(Input, After);
            Succession(Reset, After);
        }

        private ControlOutput IncreaseTimes(Flow flow)
        {
            timesTriggered++;

            int timesToTrigger = (int)flow.GetValue(Times);

            if (timesTriggered <= timesToTrigger)
            {
                return Exit;
            }
            else
            {
                return After;
            }
        }

        private ControlOutput ResetTimes(Flow flow)
        {
            timesTriggered = 0;
            return null;
        }
    }
}
