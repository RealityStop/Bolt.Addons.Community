using System.Reflection;
using System.Collections.Generic;
using Bolt;
using Ludiq;
using UnityEngine;

namespace Bolt.Addons.Community.DefinedEvents
{

    [UnitCategory("Community/Events")]
    [UnitTitle("Trigger Universal Defined Event")]
    [TypeIcon(typeof(BoltUnityEvent))]
    public class TriggerUniversalDefinedEvent : Unit
    {

        [Inspectable, UnitHeaderInspectable("Event Type")]
        public System.Type eventType;


        [DoNotSerialize]
        public List<ValueInput> inputPorts { get; } = new List<ValueInput>();

        /// <summary>
        /// The entry point to trigger the event.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter { get; private set; }

        /// <summary>
        /// The action to do after the event has been triggered.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit { get; private set; }

        [DoNotSerialize]
        private ReflectedInfo Info;

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Trigger);

            exit = ControlOutput(nameof(exit));


            BuildFromInfo();

            Succession(enter, exit);
        }

        private void BuildFromInfo()
        {
            inputPorts.Clear();
            if (eventType == null)
                return;

            Info = ReflectedInfo.For(eventType);
            foreach (var field in Info.reflectedFields)
            {
                inputPorts.Add(ValueInput(field.Value.FieldType, field.Value.Name));
            }


            foreach (var property in Info.reflectedProperties)
            {
                inputPorts.Add(ValueInput(property.Value.PropertyType, property.Value.Name));
            }
        }

        private ControlOutput Trigger(Flow flow)
        {

            if (eventType == null) return exit;

            var eventInstance = System.Activator.CreateInstance(eventType);

            for (var i = 0; i < inputPorts.Count; i++)
            {
                var inputPort = inputPorts[i];
                var key = inputPort.key;
                var value = flow.GetValue(inputPort);
                if (Info.reflectedFields.ContainsKey(key))
                {
                    var reflectedField = Info.reflectedFields[key];
                    reflectedField.SetValue(eventInstance, value);
                }
                else if (Info.reflectedProperties.ContainsKey(key))
                {
                    var reflectedProperty = Info.reflectedProperties[key];
                    reflectedProperty.SetValue(eventInstance, value);
                }
            }

            UniversalDefinedEvent.Trigger(eventInstance);

            return exit;
        }
    }

}