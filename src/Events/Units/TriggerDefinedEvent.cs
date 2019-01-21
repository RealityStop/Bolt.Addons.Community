using System.Reflection;
using System.Collections.Generic;
using Bolt;
using Ludiq;
using UnityEngine;
using Bolt.Addons.Community.DefinedEvents.Support;

namespace Bolt.Addons.Community.DefinedEvents.Units
{
    [RenamedFrom("Bolt.Addons.Community.DefinedEvents.TriggerDefinedEvent")]
    [UnitCategory("Community/Events")]
    [UnitTitle("Trigger Defined Event")]
    [TypeIcon(typeof(BoltUnityEvent))]
    public class TriggerDefinedEvent : Unit
    {
        #region Event Type Handling

        [SerializeAs(nameof(eventType))]
        private System.Type _eventType;


        /// <summary>
        /// The event type that will trigger this event.
        /// </summary>
        [DoNotSerialize]
        //[UnitHeaderInspectable("Event Type")]
        [InspectableIf(nameof(IsNotRestricted))]
        public System.Type eventType
        {
            get
            {
                return _eventType;
            }
            set
            {
                _eventType = value;
            }
        }

        /// <summary>
        /// The event type that will trigger this event.
        /// </summary>
        [DoNotSerialize]
        [UnitHeaderInspectable("Event Type")]
        [InspectableIf(nameof(IsRestricted))]
        [Ludiq.TypeFilter(TypesMatching.AssignableToAll, typeof(IDefinedEvent))]
        public System.Type restrictedEventType
        {
            get
            {
                return _eventType;
            }
            set
            {
                _eventType = value;
            }
        }


        public bool IsRestricted
        {
            get { return CommunityOptionFetcher.DefinedEvent_RestrictEventTypes; }
        }

        public bool IsNotRestricted
        {
            get { return !IsRestricted; }
        }

        #endregion


        [DoNotSerialize]
        [PortLabel("Event Target")]
        public ValueInput zzzEventTarget { get; private set; }


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

            zzzEventTarget = ValueInput<GameObject>(nameof(zzzEventTarget), null).NullMeansSelf();

            BuildFromInfo();

            Requirement(zzzEventTarget, enter);
            Succession(enter, exit);
        }

        private void BuildFromInfo()
        {
            inputPorts.Clear();
            if (_eventType == null)
                return;

            Info = ReflectedInfo.For(_eventType);
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

            if (_eventType == null) return exit;

            var eventInstance = System.Activator.CreateInstance(_eventType);

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

            TargettedDefinedEvent.Trigger(flow.GetValue<GameObject>(zzzEventTarget), eventInstance);

            return exit;
        }
    }
}