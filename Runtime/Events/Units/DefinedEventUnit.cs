using Bolt.Addons.Community.DefinedEvents.Support;
using Bolt.Addons.Community.DefinedEvents.Support.Internal;
using Bolt.Addons.Community.Utility;
using Ludiq;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;



namespace Bolt.Addons.Community.DefinedEvents.Units
{
    /// <summary>
    /// Listens for an event by type, rather than by name.  In other respects, it acts similar to the built-in Custom Unit
    /// </summary>
    [UnitCategory("Events")]
    [UnitTitle("Defined Event")]
    [RenamedFrom("Bolt.Addons.Community.DefinedEvents.Units.DefinedEvent")]
    public class DefinedEventUnit : GameObjectEventUnit<DefinedEventArgs>, IDefinedEventUnit
    {
		public override Type MessageListenerType => null;
        const string EventName = "OnDefinedEvent";

        #region Event Type Handling
        [SerializeAs(nameof(eventType))]
        private System.Type _eventType;
        

        [DoNotSerialize]
        [InspectableIf(nameof(IsNotRestricted))]
        public System.Type eventType
        {
            get {
                return _eventType; }
            set
            {
                _eventType = value;
            }
        }

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
        public List<ValueOutput> outputPorts { get; } = new List<ValueOutput>();

        [DoNotSerialize]
        private ReflectedInfo Info;

        protected override string hookName => EventName;



        protected override bool ShouldTrigger(Flow flow, DefinedEventArgs args)
        {
            return args.eventData.GetType() == _eventType;
        }


        protected override void Definition()
        {
            base.Definition();

            BuildFromInfo();
        }



        private void BuildFromInfo()
        {
            outputPorts.Clear();
            if (_eventType == null)
                return;

            Info = ReflectedInfo.For(_eventType);
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
        public override EventHook GetHook(GraphReference reference)
        {
            var refData = reference.GetElementData<Data>(this);
            return ConstructHook(refData.target, _eventType);
        }

        private static EventHook ConstructHook(GameObject target, Type eventType)
        {
            EventHook hook;
            if (DefinedEventSupport.IsOptimized())
                hook = new EventHook(EventName, target, eventType.GetTypeInfo().FullName);
            else
                hook = new EventHook(EventName, target);
            return hook;
        }


        public static void Trigger(GameObject target,object eventData)
        {
            var eventHook = ConstructHook(target, eventData.GetType());
            EventBus.Trigger(eventHook, new DefinedEventArgs(eventData));
        }

        

        public static IDisposable RegisterListener<T>(GameObject target, Action<T> onEvent) 
        {
            var eventHook = ConstructHook(target, typeof(T));
            Action<DefinedEventArgs> action = (x) => {
                if (x.eventData.GetType() == typeof(T))
                    onEvent((T)x.eventData);
            };
            EventBus.Register<DefinedEventArgs>(eventHook, action);

            return Disposable.Create(() => { EventBus.Unregister(eventHook, action); });
        }
    }
}