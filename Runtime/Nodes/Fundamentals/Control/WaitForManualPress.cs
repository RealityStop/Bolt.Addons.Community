using System.Collections;
using Unity.VisualScripting.Community.Utility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("WaitForManualPress")]
    [UnitTitle("Wait For Press")]
    [UnitCategory("Community/Control")]
    [TypeIcon(typeof(WaitUnit))]
    public class WaitForManualPress : Unit, IGraphElementWithData
    {
        private class Data : IGraphElementData
        {
            public bool isWaiting;

            public bool isCoroutine;
        }

        [NodeButton("Trigger")]
        [UnitHeaderInspectable]
        public NodeButton button;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput input;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput output;

        protected override void Definition()
        {
            input = ControlInputCoroutine(nameof(input), Wait, WaitCoroutine);
            output = ControlOutput(nameof(output));

            Succession(input, output);
        }

        private ControlOutput Wait(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);
            data.isWaiting = true;
            data.isCoroutine = false;
            return null;
        }

        private IEnumerator WaitCoroutine(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);
            data.isCoroutine = true;

            if (data.isWaiting)
                yield break;

            data.isWaiting = true;
            yield return new WaitWhile(() => data.isWaiting);
            yield return output;
        }

        public void Trigger(GraphReference reference)
        {
            if (!reference.hasData) return;

            var data = reference.GetElementData<Data>(this);

            if (data.isWaiting)
            {
                data.isWaiting = false;
                if (!data.isCoroutine)
                {
                    Flow flow = Flow.New(reference);
                    flow.Run(output);
                }
            }
        }

        public IGraphElementData CreateData()
        {
            return new Data();
        }
    }

}