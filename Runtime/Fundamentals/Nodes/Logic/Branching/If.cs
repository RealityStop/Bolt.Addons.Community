using UnityEngine;
using Unity.VisualScripting;
using System.Collections;
using System;

namespace Unity.VisualScripting.Community
{
    [UnitTitle("If")]
    [UnitCategory("Community/Control")]
    [TypeIcon(typeof(Unity.VisualScripting.If))]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.If")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.BetterIf")]
    public class BetterIf : Unit
    {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput In;

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput Condition;

        [DoNotSerialize]
        public ControlOutput True;
        [DoNotSerialize]
        public ControlOutput False;
        [DoNotSerialize]
        [PortLabel("Next")]
        public ControlOutput Finished;

        protected override void Definition()
        {
            In = ControlInputCoroutine(nameof(In), Enter, EnterCoroutine);
            True = ControlOutput(nameof(True));
            False = ControlOutput(nameof(False));
            Finished = ControlOutput(nameof(Finished));

            Condition = ValueInput<bool>(nameof(Condition));

            Succession(In, True);
            Succession(In, False);
            Succession(In, Finished);
            Requirement(Condition, In);
        }

        private IEnumerator EnterCoroutine(Flow flow)
        {
            bool condition = flow.GetValue<bool>(Condition);

            if (condition)
            {
                yield return True;
            }
            else
            {
                yield return False;
            }

            yield return Finished;
        }

        private ControlOutput Enter(Flow flow)
        {
            bool condition = flow.GetValue<bool>(Condition);

            if (condition)
            {
                flow.Invoke(True);
            }
            else
            {
                flow.Invoke(False);
            }

            return Finished;

        }
    }
}
