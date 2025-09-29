using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("FlowToCoroutine")]
    [UnitTitle("FlowToCoroutine")]
    [UnitCategory("Community\\Control")]
    [TypeIcon(typeof(Coroutine))]
    public class FlowToCoroutine : Unit
    {
        [DoNotSerialize]
        public ControlInput In;
        [DoNotSerialize]
        public ControlOutput Converted;
        [DoNotSerialize]
        [PortLabel("Flow")]
        public ControlOutput normalFlow;

        protected override void Definition()
        {
            In = ControlInput("In", Convert);
            Converted = ControlOutput("Coroutine");
            normalFlow = ControlOutput("Flow");

            Succession(In, Converted);
            Succession(In, normalFlow);
        }

        private ControlOutput Convert(Flow flow)
        {
            var graphRef = flow.stack.ToReference();

            if (flow.isCoroutine)
            {
                Debug.LogWarning(
                    $"[FlowToCoroutine] This unit is meant to convert a normal flow into a coroutine flow. " +
                    $"Using it inside an existing coroutine flow has no effect and is unnecessary. " +
                    $"Unit: {this}",
                    flow.stack.gameObject
                );

                return Converted;
            }
            else
            {
                var convertedflow = Flow.New(graphRef);
                convertedflow.StartCoroutine(Converted);
                return normalFlow;
            }
        }
    }
}