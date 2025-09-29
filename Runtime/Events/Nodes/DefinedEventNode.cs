using Unity.VisualScripting.Community.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// Listens for an event by type, rather than by name. In other respects, it acts similar to the built-in Custom Unit.
    /// </summary>
    [UnitCategory("Events/Community")]
    [UnitTitle("Defined Event")]
    [RenamedFrom("Bolt.Addons.Community.DefinedEvents.Units.DefinedEvent")]
    [RenamedFrom("Bolt.Addons.Community.DefinedEvents.Units.DefinedEventUnit")]
    public class DefinedEventNode : GameObjectEventUnit<DefinedEventArgs>, IDefinedEventNode
    {
        // Old serialized field
        [SerializeAs("eventType")]
        [Obsolete]
        private System.Type _legacyEventType;

        // New serialized field
        [SerializeAs("NeweventType")]
        private DefinedEventType _eventType;

        [DoNotSerialize]
        [InspectableIf(nameof(IsNotRestricted))]
        [InspectorLabel("EventType")]
#if !RESTRICT_EVENT_TYPES
        [UnitHeaderInspectable]
#endif
        public DefinedEventType EventType
        {
            get => _eventType;
            set => _eventType = value;
        }

        [DoNotSerialize]
        [InspectableIf(nameof(IsRestricted))]
        [InspectorLabel("EventType")]
        [TypeFilter(TypesMatching.AssignableToAll, typeof(IDefinedEvent))]
#if RESTRICT_EVENT_TYPES
        [UnitHeaderInspectable]
#endif
        public DefinedEventType RestrictedEventType
        {
            get => _eventType;
            set => _eventType = value;
        }

        public bool IsRestricted => CommunityOptionFetcher.DefinedEvent_RestrictEventTypes;
        public bool IsNotRestricted => !IsRestricted;

        [DoNotSerialize] public List<ValueOutput> outputPorts { get; } = new List<ValueOutput>();
        [DoNotSerialize] private ReflectedInfo Info;

        public override Type MessageListenerType => null;
        protected override string hookName => CommunityEvents.OnDefinedEvent;

        protected override bool ShouldTrigger(Flow flow, DefinedEventArgs args)
        {
            return args.eventData.GetType() == _eventType?.type;
        }

        protected override void Definition()
        {
            base.Definition();

            // One-time migration from old serialized Type
#pragma warning disable
            if (_legacyEventType != null)
            {
                _eventType = new DefinedEventType(_legacyEventType);
                _legacyEventType = null;
            }
#pragma warning restore
            _eventType ??= new DefinedEventType();

            BuildFromInfo();
        }

        private void BuildFromInfo()
        {
            outputPorts.Clear();
            if (_eventType?.type == null)
                return;

            if (IsRestricted)
            {
                Info = ReflectedInfo.For(_eventType.type);
                foreach (var field in Info.reflectedFields)
                    outputPorts.Add(ValueOutput(field.Value.FieldType, field.Value.Name));
                foreach (var property in Info.reflectedProperties)
                    outputPorts.Add(ValueOutput(property.Value.PropertyType, property.Value.Name));
            }
            else
            {
                outputPorts.Add(ValueOutput(_eventType.type, "Value"));
            }
        }

        protected override void AssignArguments(Flow flow, DefinedEventArgs args)
        {
            if (IsRestricted)
            {
                for (var i = 0; i < outputPorts.Count; i++)
                {
                    var outputPort = outputPorts[i];
                    var key = outputPort.key;
                    if (Info.reflectedFields.TryGetValue(key, out var f))
                        flow.SetValue(outputPort, f.GetValue(args.eventData));
                    else if (Info.reflectedProperties.TryGetValue(key, out var p))
                        flow.SetValue(outputPort, p.GetValue(args.eventData));
                }
            }
            else
            {
                if (outputPorts.Count > 0)
                    flow.SetValue(outputPorts[0], args.eventData);
            }
        }

        public override EventHook GetHook(GraphReference reference)
        {
            var refData = reference.GetElementData<Data>(this);
            return ConstructHook(refData.target, _eventType.type);
        }

        private static EventHook ConstructHook(GameObject target, Type eventType)
        {
            if (DefinedEventSupport.IsOptimized())
                return new EventHook(CommunityEvents.OnDefinedEvent, target, eventType.GetTypeInfo().FullName);
            else
                return new EventHook(CommunityEvents.OnDefinedEvent, target);
        }

        public static void Trigger(GameObject target, object eventData)
        {
            var eventHook = ConstructHook(target, eventData.GetType());
            EventBus.Trigger(eventHook, new DefinedEventArgs(eventData));
        }

        public static IDisposable RegisterListener<T>(GameObject target, Action<T> onEvent)
        {
            var eventHook = ConstructHook(target, typeof(T));
            Action<DefinedEventArgs> action = (x) =>
            {
                if (x.eventData.GetType() == typeof(T))
                    onEvent((T)x.eventData);
            };
            EventBus.Register(eventHook, action);

            return Disposable.Create(() => { EventBus.Unregister(eventHook, action); });
        }
    }
}