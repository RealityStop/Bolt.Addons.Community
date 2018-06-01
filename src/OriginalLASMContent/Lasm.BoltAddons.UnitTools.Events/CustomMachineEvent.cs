using System;
using Ludiq;
using UnityEngine;

namespace Bolt
{
    /// <summary>
    /// A special named event with any amount of parameters called manually with the 'Trigger Custom Event' unit.
    /// </summary>
    [UnitCategory("Events")]
    [UnitOrder(0)]
    public sealed class CustomMachineEvent : MachineEvent
    {
        [SerializeAs(nameof(argumentCount))]
        private int _argumentCount;

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

        FlowMachine flowmachine;
        StateMachine statemachine;

        /// <summary>
        /// The name of the event.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput name { get; private set; }

        

        public override void Initialize()
        {
            base.Initialize();

            if ((FlowMachine)this.graph.owner != null)
            {
                flowmachine = (FlowMachine)this.graph.owner;
            } else
            {
                statemachine = (StateMachine)this.graph.owner;
            }

        }

        protected override bool ShouldTrigger()
        {
            throw InvalidArgumentCount(0);
        }

        protected override bool ShouldTrigger(object argument)
        {
            return CompareNames(name, (string)argument);
        }

        protected override bool ShouldTrigger(object[] arguments)
        {
            return CompareNames(name, (string)arguments[0]);
        }

        protected override void Definition()
        {
            base.Definition();

            ArgumentCount(argumentCount + 1);

            name = ValueInput(nameof(name), string.Empty);

            for (var i = 0; i < argumentCount; i++)
            {
                var _i = i; // Cache outside closure

                ValueOutput("argument_" + i, (recursion) => arguments[1 + _i]);
            }
        }

        public static object[] CombineArgs(string name, object[] args)
        {
            var fullArgs = new object[args.Length + 1];
            fullArgs[0] = name;
            Array.Copy(args, 0, fullArgs, 1, args.Length);
            return fullArgs;
        }

        public static void Trigger(FlowMachine target, string name, params object[] args)
        {
            Trigger<CustomMachineEvent>(target, CombineArgs(name, args));
        }
    }
}