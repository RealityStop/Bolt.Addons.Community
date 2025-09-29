using System;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// Triggers a Global Event to all Global Defined Events listening for this type.
    /// </summary>
    [UnitCategory("Events/Community")]
    [UnitTitle("Trigger Global Defined Event")]
    [TypeIcon(typeof(BoltUnityEvent))]
    [RenamedFrom("Bolt.Addons.Community.DefinedEvents.Units.TriggerGlobalDefinedEvent")]
    public class TriggerGlobalDefinedEvent : Unit
    {
        // Old field (kept only for migration)
        [SerializeAs("eventType")]
        [Obsolete]
        private System.Type _legacyEventType;

        // New serialization
        [SerializeAs("NeweventType")]
        private DefinedEventType _eventType;

        [DoNotSerialize, InspectorLabel("Event Type")]
        [InspectableIf(nameof(IsNotRestricted))]
#if !RESTRICT_EVENT_TYPES
        [UnitHeaderInspectable]
#endif
        public DefinedEventType EventType
        {
            get => _eventType;
            set => _eventType = value;
        }

        [DoNotSerialize, InspectorLabel("Event Type")]
        [InspectableIf(nameof(IsRestricted))]
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


        [DoNotSerialize]
        public List<ValueInput> inputPorts { get; } = new List<ValueInput>();

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
            // One-time migration from old serialized Type to DefinedEventType
#pragma warning disable
            if (_legacyEventType != null)
            {
                _eventType = new DefinedEventType(_legacyEventType);
                _legacyEventType = null;
            }
#pragma warning restore
            _eventType ??= new DefinedEventType();

            enter = ControlInput(nameof(enter), Trigger);
            exit = ControlOutput(nameof(exit));

            BuildFromInfo();

            Succession(enter, exit);
        }

        private void BuildFromInfo()
        {
            inputPorts.Clear();
            if (_eventType?.type == null) return;

            if (IsRestricted)
            {
                Info = ReflectedInfo.For(_eventType);
                foreach (var f in Info.reflectedFields.Values)
                    AddInputForType(f.FieldType, f.Name);
                foreach (var p in Info.reflectedProperties.Values)
                    AddInputForType(p.PropertyType, p.Name);
            }
            else
            {
                var input = ValueInput(_eventType, "Value");
                input.SetDefaultValue(_eventType.type.PseudoDefault());
                inputPorts.Add(input);
            }
        }

        private void AddInputForType(System.Type type, string name)
        {
            if (type == typeof(bool)) inputPorts.Add(ValueInput(name, false));
            else if (type == typeof(int)) inputPorts.Add(ValueInput(name, 0));
            else if (type == typeof(float)) inputPorts.Add(ValueInput(name, 0f));
            else if (type == typeof(string)) inputPorts.Add(ValueInput(name, ""));
            else if (ComponentHolderProtocol.IsComponentHolderType(type))
            {
                var input = ValueInput(type, name);
                input.SetDefaultValue(null);
                inputPorts.Add(input);
            }
            else
                inputPorts.Add(ValueInput(type, name));
        }

        private ControlOutput Trigger(Flow flow)
        {
            if (_eventType?.type == null) return exit;

            if (IsRestricted)
            {
                var instance = System.Activator.CreateInstance(_eventType);

                foreach (var port in inputPorts)
                {
                    var key = port.key;
                    var value = flow.GetValue(port);
                    if (Info.reflectedFields.TryGetValue(key, out var f)) f.SetValue(instance, value);
                    else if (Info.reflectedProperties.TryGetValue(key, out var p)) p.SetValue(instance, value);
                }

                GlobalDefinedEventNode.Trigger(instance);
            }
            else
            {
                GlobalDefinedEventNode.Trigger(flow.GetValue(inputPorts[0]));
            }

            return exit;
        }
    }

}