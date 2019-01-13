using Ludiq;
using System.Collections.Generic;
using System.Reflection;
/// <summary>
/// Universal event.
/// 
/// Changing the event type _does not work in play mode_
/// </summary>
/// 
namespace Bolt.Addons.Community.DefinedEvents
{
    [UnitCategory("Events")]
    [UnitTitle("On Defined Event")]
    public class DefinedEvent : EventUnit<DefinedEventArgs>
    {
        const string EventName = "OnDefinedEvent";


        //[Inspectable, UnitHeaderInspectable]
        //public System.Type eventType;

        [SerializeAs(nameof(lastEventType))]
        private System.Type _lastEventType;

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
            set
            {
                var isDirty = _eventType != null;
                //if (isDirty) Define();

                _lastEventType = _eventType;
                _eventType = value;
            }
        }

        [DoNotSerialize]
        public System.Type lastEventType
        {
            get
            {
                return _lastEventType;
            }
        }


        [DoNotSerialize]
        public List<ValueOutput> outputPorts { get; } = new List<ValueOutput>();

        [DoNotSerialize]
        private ReflectedInfo Info;

        //protected override bool register => false;
        protected override bool register => true;

        public override void StartListening(GraphStack stack)
        {
            // FIXME: StartListening only gets retriggered if an input changes.

            //var wasListening = data.isListening;

            //if (lastEventType != null && eventType != lastEventType)
            //{
            //    if (wasListening)
            //    {
            //        var tag = lastEventType.GetTypeInfo().FullName;
            //        var eventHook = new EventHook("OnUniversalEvent", null, tag);

            //        EventBus.Unregister(eventHook, data.handler);

            //        StopListening(stack);

            //        _lastEventType = eventType;
            //    }
            //}

            base.StartListening(stack);
        }

        public override void StopListening(GraphStack stack)
        {
            base.StopListening(stack);
        }

        protected override bool ShouldTrigger(Flow flow, DefinedEventArgs args)
        {
            // FIXME: can't seem to catch when `eventType` was changed
            // so we can't force a StopListening call.
            // The only time this matters is during modifications during play mode
            //
            // If the incoming event data doesn't match our set event type,
            // do not trigger the event.
            return args.eventData.GetType() == eventType;
        }

        public override EventHook GetHook(GraphReference reference)
        {
            if (eventType == null)
            {
                return new EventHook(EventName);
            }

            var tag = eventType.GetTypeInfo().FullName;
            return new EventHook(EventName, null, tag);
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
            var tag = eventData.GetType().GetTypeInfo().FullName;
            var eventHook = new EventHook(EventName, null, tag);
            EventBus.Trigger(eventHook, new DefinedEventArgs(eventData));
        }
    }
}