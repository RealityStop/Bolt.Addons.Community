using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.TriggerNextTask")]
    /// <summary>
    /// Triggers A Wait For Event node if it one is waiting.
    /// </summary>
    [UnitTitle("SendTaskEvent")]
    [UnitCategory("Events\\Community")]
    [TypeIcon(typeof(EventUnit<EmptyEventArgs>))]
    [Obsolete]
    public class TriggerNextTask : Unit
    {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput inputTrigger { get; private set; }

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput outputTrigger { get; private set; }

        protected override void Definition()
        {

            inputTrigger = ControlInput(nameof(inputTrigger), Trigger);

            outputTrigger = ControlOutput(nameof(outputTrigger));

            Succession(inputTrigger, outputTrigger);
        }

        //Send the Event WaitForEvent.
        private ControlOutput Trigger(Flow flow)
        {
            EventBus.Trigger(CommunityEvents.WaitForEvent);
            return outputTrigger;
        }
    }
}
