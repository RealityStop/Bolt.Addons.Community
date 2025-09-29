using UnityEngine;
using Unity.VisualScripting;
using System;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.WaitForNode")]
    [UnitCategory("Events\\Community")]
    [UnitTitle("Wait For Task Event")]
    [TypeIcon(typeof(CustomEvent))]
    [Obsolete]
    public class WaitForNode : Unit
    {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit;

        private bool isWaiting;

        private GraphReference stack;

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Wait);
            exit = ControlOutput(nameof(exit));

            Succession(enter, exit);
        }

        private ControlOutput Wait(Flow flow)
        {
            if (isWaiting)
                return null;

            stack = flow.stack.ToReference();

            isWaiting = true;
            EventBus.Register<EmptyEventArgs>(CommunityEvents.WaitForEvent, OnEventTriggered);
            return null;
        }

        private void OnEventTriggered(EmptyEventArgs args)
        {
            if (isWaiting)
            {
                isWaiting = false;
                Flow flow = Flow.New(stack);
                flow.Run(exit);
            }
        }
    }
}
