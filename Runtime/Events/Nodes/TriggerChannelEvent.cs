using System;
using UnityEngine;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.TriggerChannelEvent")]
    [UnitCategory("Events\\Community")]
    [UnitTitle("Trigger Channel")]
    [TypeIcon(typeof(CustomEvent))]
    public class TriggerChannelEvent : Unit
    {
        [DoNotSerialize]
        [PortLabelHidden]
        [InspectorLabel("Enter")]
        public ControlInput InputTrigger { get; private set; }

        [DoNotSerialize]
        [PortLabelHidden]
        [InspectorLabel("Exit")]
        public ControlOutput OutputTrigger { get; private set; }

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput channel;

        protected override void Definition()
        {
            InputTrigger = ControlInput(nameof(InputTrigger), Trigger);
            channel = ValueInput<Channel>(nameof(channel), default);
            OutputTrigger = ControlOutput(nameof(OutputTrigger));
            Succession(InputTrigger, OutputTrigger);
        }

        private ControlOutput Trigger(Flow flow)
        {
            try
            {
                var channelValue = flow.GetValue<Channel>(channel);

                EventBus.Trigger(CommunityEvents.ChannelEvent, channelValue);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                // Handle the exception or provide fallback behavior
            }

            return OutputTrigger;
        }
    }
}
