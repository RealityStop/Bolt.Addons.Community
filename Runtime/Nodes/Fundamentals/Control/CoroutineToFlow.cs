using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("CorutineConverter")]
    [RenamedFrom("Unity.VisualScripting.Community.CorutineConverter")]
    [UnitTitle("Coroutine To Flow")]
    [UnitCategory("Community\\Control")]
    [TypeIcon(typeof(Flow))]
    public class CoroutineToFlow : Unit
    {
        [DoNotSerialize]
        public ControlInput In;
        [DoNotSerialize]
        public ControlOutput Converted;
        [DoNotSerialize]
        [PortLabel("Coroutine")]
        public ControlOutput Coroutine;

        protected override void Definition()
        {
            In = ControlInput("In", Convert);
            Converted = ControlOutput("Flow");
            Coroutine = ControlOutput("Coroutine");

            Succession(In, Converted);
            Succession(In, Coroutine);
        }

        private ControlOutput Convert(Flow flow)
        {
            var GraphRef = flow.stack.ToReference();

            if (!flow.isCoroutine)
            {
                Debug.LogWarning(
                    $"[CoroutineToFlow] Tried to convert a normal (non-coroutine) flow to a coroutine. " +
                    $"This is not valid and may cause unexpected behavior. " +
                    $"Unit: {this}",
                    flow.stack.gameObject
                );
                return Converted;
            }
            else
            {
                var Convertedflow = Flow.New(GraphRef);
                Convertedflow.Run(Converted);
                return Coroutine;
            }
        }
    }

}