using Ludiq;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
/// <summary>
/// Universal event.
/// 
/// Changing the event type _does not work in play mode_
/// </summary>
/// 
namespace Bolt.Addons.Community.DefinedEvents
{
    [UnitCategory("Events")]
    [UnitTitle("On Universal Defined Event")]
    public class UniversalDefinedEvent : EventUnit<DefinedEventArgs>
    {
        const string EventName = "OnUniversalDefinedEvent";

        [SerializeAs(nameof(eventType))]
        private System.Type _eventType;

        /// <summary>
        /// The event type that will trigger this event.
        /// </summary>
        [DoNotSerialize]
        [Inspectable, UnitHeaderInspectable("Event Type")]
        public System.Type eventType
        {
            get { return _eventType; }
            set { _eventType = value; }
        }

        [DoNotSerialize]
        public List<ValueOutput> outputPorts { get; } = new List<ValueOutput>();

        [DoNotSerialize]
        private ReflectedInfo Info;

        //protected override bool register => false;
        protected override bool register => true;

        protected override bool ShouldTrigger(Flow flow, DefinedEventArgs args)
        {
            return args.eventData.GetType() == eventType;
        }

        public override EventHook GetHook(GraphReference reference)
        {
            return new EventHook(EventName);
        }

        protected override void Definition()
        {
            base.Definition();

            BuildFromInfo();
        }


        private void BuildFromInfo()
        {
            outputPorts.Clear();
            if (eventType == null)
                return;

            Info = ReflectedInfo.For(eventType);
            foreach (var field in Info.reflectedFields)
            {
                outputPorts.Add(ValueOutput(field.Value.FieldType, field.Value.Name));
            }

            foreach (var property in Info.reflectedProperties)
            {
                outputPorts.Add(ValueOutput(property.Value.PropertyType, property.Value.Name));
            }
        }

        protected override void AssignArguments(Flow flow, DefinedEventArgs args)
        {
            for (var i = 0; i < outputPorts.Count; i++)
            {
                var outputPort = outputPorts[i];
                var key = outputPort.key;
                if (Info.reflectedFields.ContainsKey(key))
                {
                    var reflectedField = Info.reflectedFields[key];
                    flow.SetValue(outputPort, reflectedField.GetValue(args.eventData));
                }
                else if (Info.reflectedProperties.ContainsKey(key))
                {
                    var reflectedProperty = Info.reflectedProperties[key];
                    flow.SetValue(outputPort, reflectedProperty.GetValue(args.eventData));
                }
            }
        }

        public static void Trigger(object eventData)
        {
            //var tag = eventData.GetType().GetTypeInfo().FullName;
            //var eventHook = new EventHook(EventName, null, tag);
            var eventHook = new EventHook(EventName);
            EventBus.Trigger(eventHook, new DefinedEventArgs(null,eventData));
        }
    }
}