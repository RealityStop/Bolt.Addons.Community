using UnityEngine;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Fundamentals
{
    /// <summary>
    /// Once triggered will wait until a Send Event node gets triggered.
    /// </summary>

    [UnitCategory("Events")]
    [UnitTitle("Wait For Task Event")]
    [TypeIcon(typeof(CustomEvent))]
    public class WaitForNode : Unit
    {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit;

        private bool isWaiting;

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Wait);
            exit = ControlOutput(nameof(exit));

            Succession(enter, exit);
        }



        private ControlOutput Wait(Flow flow)
        {
            isWaiting = true;
            WaitForEvent(flow);
            
            return null;
        }


        private ControlOutput WaitForEvent(Flow flow)
        {

            EventBus.Register<GameObject>(CommunityEvents.WaitForEvent, i =>
            {
                if (isWaiting)
                {
                    flow.Invoke(exit);

                    isWaiting = false;
                }
            });

            return null;
        }
    }
}
