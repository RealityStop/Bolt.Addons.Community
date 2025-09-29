using System.Collections;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Utility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("WaitForManualPress")]
    [UnitTitle("WaitForPress")]
    [UnitCategory("Community/Control")]
    [TypeIcon(typeof(WaitUnit))]
    public class WaitForManualPress : Unit
    {
        [NodeButton("Trigger")]
        [UnitHeaderInspectable]
        public NodeButton button;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput input;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput output;

        private bool IsWaiting;

        private GraphReference reference;

        private bool coroutine;

        protected override void Definition()
        {
            input = ControlInputCoroutine(nameof(input), Wait, WaitCoroutine);
            output = ControlOutput(nameof(output));

            Succession(input, output);
        }

        private ControlOutput Wait(Flow flow)
        {
            reference = flow.stack.ToReference();
            IsWaiting = true;
            coroutine = false;
            return null;
        }

        private IEnumerator WaitCoroutine(Flow flow)
        {
            IsWaiting = true;
            coroutine = true;
            yield return new WaitWhile(() => IsWaiting);
            yield return output;
        }

        public void Trigger(GraphReference reference)
        {
            if (IsWaiting)
            {
                IsWaiting = false;
                if (!coroutine)
                {
                    Flow flow = Flow.New(reference);
                    flow.Invoke(output);
                }
            }
        }
    }

}