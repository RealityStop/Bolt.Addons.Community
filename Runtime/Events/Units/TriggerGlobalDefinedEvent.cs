using System.Collections.Generic;
using UnityEngine;

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
        #region Event Type Handling

        [SerializeAs(nameof(eventType))]
        private System.Type _eventType;


        [DoNotSerialize]
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

        [DoNotSerialize]
        [UnitHeaderInspectable]
        [InspectableIf(nameof(IsRestricted))]
        [Unity.VisualScripting.TypeFilter(TypesMatching.AssignableToAll, typeof(IDefinedEvent))]
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
                if (field.Value.FieldType == typeof(bool))
                    inputPorts.Add(ValueInput<bool>(field.Value.Name, false));
                else if (field.Value.FieldType == typeof(int))
                    inputPorts.Add(ValueInput<int>(field.Value.Name, 0));
                else if (field.Value.FieldType == typeof(float))
                    inputPorts.Add(ValueInput<float>(field.Value.Name, 0.0f));
                else if (field.Value.FieldType == typeof(string))
                    inputPorts.Add(ValueInput<string>(field.Value.Name, ""));
                else if (field.Value.FieldType == typeof(GameObject))
                    inputPorts.Add(ValueInput<GameObject>(field.Value.Name, null).NullMeansSelf());
                else
                    inputPorts.Add(ValueInput(field.Value.FieldType, field.Value.Name));
            }


            foreach (var property in Info.reflectedProperties)
            {
                if (property.Value.PropertyType == typeof(bool))
                    inputPorts.Add(ValueInput<bool>(property.Value.Name, false));
                else if (property.Value.PropertyType == typeof(int))
                    inputPorts.Add(ValueInput<int>(property.Value.Name, 0));
                else if (property.Value.PropertyType == typeof(float))
                    inputPorts.Add(ValueInput<float>(property.Value.Name, 0.0f));
                else if (property.Value.PropertyType == typeof(string))
                    inputPorts.Add(ValueInput<string>(property.Value.Name, ""));
                else if (property.Value.PropertyType == typeof(GameObject))
                    inputPorts.Add(ValueInput<GameObject>(property.Value.Name, null).NullMeansSelf());
                else
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

            GlobalDefinedEventNode.Trigger(eventInstance);

            return exit;
        }
    }

}