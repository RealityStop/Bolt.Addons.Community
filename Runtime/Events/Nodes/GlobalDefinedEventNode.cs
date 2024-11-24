
using Unity.VisualScripting.Community.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// Listens for an event by type, rather than by name.  In other respects, it acts similar
    /// to the built-in Custom Unit.  This variation listens for events triggered by any
    /// Trigger Global Defined Event.
    /// </summary>
    [UnitCategory("Events/Community")]
    [UnitTitle("Global Defined Event")]
    [RenamedFrom("Bolt.Addons.Community.DefinedEvents.Units.GlobalDefinedEvent")]
    [RenamedFrom("Bolt.Addons.Community.DefinedEvents.Units.GlobalDefinedEventUnit")]
    public class GlobalDefinedEventNode : EventUnit<DefinedEventArgs>, IDefinedEventNode
    {
        const string EventName = "OnGlobalDefinedEvent";

        #region Previous Event Type Handling (for backward compatibility)

        [SerializeAs(nameof(eventType))]
        private Type _eventType;

        [DoNotSerialize]
        public Type eventType
        {
            get { return _eventType; }
            set { _eventType = value; }
        }

        [DoNotSerialize]
        public Type restrictedEventType
        {
            get { return _eventType; }
            set { _eventType = value; }
        }

        #endregion

        #region New Event Type Handling

        [SerializeAs(nameof(NeweventType))]
        private IDefinedEventType New_eventType;

        [DoNotSerialize]
        public IDefinedEventType NeweventType
        {
            get { return New_eventType; }
            set { New_eventType = value; }
        }

        [DoNotSerialize]
        [UnitHeaderInspectable]
        [InspectableIf(nameof(IsRestricted))]
        public IDefinedEventType NewrestrictedEventType
        {
            get { return New_eventType; }
            set { New_eventType = value; }
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
        public List<ValueOutput> outputPorts { get; } = new List<ValueOutput>();

        [DoNotSerialize]
        private ReflectedInfo Info;
        public override bool canDefine => NeweventType != null && NeweventType.type != null;
        protected override bool register => true;

        protected override bool ShouldTrigger(Flow flow, DefinedEventArgs args)
        {
            return args.eventData.GetType() == NeweventType.type;
        }

        public override EventHook GetHook(GraphReference reference)
        {
            return ConstructHook(NeweventType.type);
        }

        protected override void Definition()
        {
            base.Definition();

            // For backward compatibility, convert the Type to IDefinedEventType
            if (restrictedEventType != null)
            {
                NewrestrictedEventType = new IDefinedEventType(restrictedEventType);
                restrictedEventType = null;
            }

            if (NewrestrictedEventType == null)
            {
                NewrestrictedEventType = new IDefinedEventType();
            }

            BuildFromInfo();
        }


        private void BuildFromInfo()
        {
            outputPorts.Clear();
            if (NeweventType == null)
                return;

            Info = ReflectedInfo.For(NeweventType.type);
            if (Info == null)
                return;

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

        private static EventHook ConstructHook(Type eventType)
        {
            EventHook hook;
            if (DefinedEventSupport.IsOptimized())
                hook = new EventHook(EventName, tag: eventType.GetTypeInfo().FullName);
            else
                hook = new EventHook(EventName);
            return hook;
        }

        public static void Trigger(object eventData)
        {
            //var tag = eventData.GetType().GetTypeInfo().FullName;
            //var eventHook = new EventHook(EventName, null, tag);
            EventHook hook = ConstructHook(eventData.GetType());
            EventBus.Trigger(hook, new DefinedEventArgs(eventData));
        }

        public static IDisposable RegisterListener<T>(Action<T> onEvent)
        {
            var eventHook = ConstructHook(typeof(T));
            Action<DefinedEventArgs> action = (x) =>
            {
                if (x.eventData.GetType() == typeof(T))
                    onEvent((T)x.eventData);
            };
            EventBus.Register<DefinedEventArgs>(eventHook, action);

            return Disposable.Create(() => { EventBus.Unregister(eventHook, action); });
        }
    }
}
