using System;
using UnityEngine;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Fundamentals
{
    [UnitCategory("Events\\Community")]
    [UnitTitle("TriggerChannelEvent")]
    [TypeIcon(typeof(CustomEvent))]
    public class TriggerChannelEvent : Unit
    {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput InputTrigger { get; private set; }

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput OutputTrigger { get; private set; }

        [DoNotSerialize]
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

                switch (channelValue)
                {
                    case Channel.Channel1:
                        EventBus.Trigger(CommunityEvents.ChannelEvent1);
                        break;
                    case Channel.Channel2:
                        EventBus.Trigger(CommunityEvents.ChannelEvent2);
                        break;
                    case Channel.Channel3:
                        EventBus.Trigger(CommunityEvents.ChannelEvent3);
                        break;
                    case Channel.Channel4:
                        EventBus.Trigger(CommunityEvents.ChannelEvent4);
                        break;
                    case Channel.Channel5:
                        EventBus.Trigger(CommunityEvents.ChannelEvent5);
                        break;
                    case Channel.Channel6:
                        EventBus.Trigger(CommunityEvents.ChannelEvent6);
                        break;
                    case Channel.Channel7:
                        EventBus.Trigger(CommunityEvents.ChannelEvent7);
                        break;
                    case Channel.Channel8:
                        EventBus.Trigger(CommunityEvents.ChannelEvent8);
                        break;
                    case Channel.Channel9:
                        EventBus.Trigger(CommunityEvents.ChannelEvent9);
                        break;
                    case Channel.Channel10:
                        EventBus.Trigger(CommunityEvents.ChannelEvent10);
                        break;
                    default:
                        Debug.LogWarning("Unknown channel value: " + channelValue);
                        break;
                }
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
