using System;
using Unity.VisualScripting;
using UnityEngine;
namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(ChannelEvent))]
    public class ChannelEventDescriptor : UnitDescriptor<ChannelEvent>
    {
        public ChannelEventDescriptor(ChannelEvent unit) : base(unit) { }

        protected override void DefinedPort(IUnitPort port, UnitPortDescription description)
        {
            base.DefinedPort(port, description);
            switch (port.key)
            {
                case "trigger":
                    description.summary = "Triggers when any" +
                        "<b>TriggerChannelEvent</b> node gets Triggered no matter the channel";
                    description.label = "Trigger";
                    break;
            }
        }

        protected override string DefinedSummary()
        {
            return "You cannot change the channel after" +
                " the game has started changing it has no effect";
        }
    }
}