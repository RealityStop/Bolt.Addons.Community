using System.Collections.Generic;
using System.Linq;
using Ludiq;
using UnityEngine;

namespace Bolt
{
    /// <summary>
    /// Triggers a custom event.
    /// </summary>
    [UnitSurtitle("Custom Machine Event")]
    [UnitShortTitle("Trigger")]
    [TypeIcon(typeof(CustomEvent))]
    [UnitCategory("Events")]
    [UnitOrder(1)]
    public sealed class TriggerCustomMachineEvent : Unit
    {
        public TriggerCustomMachineEvent() { }

        [SerializeAs(nameof(argumentCount))]
        private int _argumentCount;

        private List<ValueInput> arguments;

        [DoNotSerialize]
        [Inspectable, UnitHeaderInspectable("Arguments")]
        public int argumentCount
        {
            get
            {
                return _argumentCount;
            }
            set
            {
                _argumentCount = Mathf.Clamp(value, 0, 10);
            }
        }

        /// <summary>
        /// The entry point to trigger the event.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter { get; private set; }

        /// <summary>
        /// The name of the event.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput name { get; private set; }

        /// <summary>
        /// The target of the event.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput target { get; private set; }

        /// <summary>
        /// The action to do after the event has been triggered.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit { get; private set; }

        FlowMachine flowmachine;
        StateMachine statemachine;

        public override void Initialize()
        {
            base.Initialize();

            if ((FlowMachine)this.graph.owner != null)
            {
                flowmachine = (FlowMachine)this.graph.owner;
            }
            else
            {
                statemachine = (StateMachine)this.graph.owner;
            }

        }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Trigger);

            exit = ControlOutput(nameof(exit));

            name = ValueInput(nameof(name), string.Empty);

            target = ValueInput<FlowMachine>(nameof(target), flowmachine);

            arguments = new List<ValueInput>();

            for (var i = 0; i < argumentCount; i++)
            {
                var argument = ValueInput<object>("argument_" + i);
                arguments.Add(argument);
                Relation(argument, enter);
            }

            Relation(enter, exit);
            Relation(name, enter);
            Relation(target, enter);
        }

       

        private void Trigger(Flow flow)
        {
            CustomMachineEvent.Trigger(target.GetValue<FlowMachine>(), name.GetValue<string>(), arguments.Select(arg => arg.GetConvertedValue()).ToArray());
            flow.Invoke(exit);
        }
    }
}