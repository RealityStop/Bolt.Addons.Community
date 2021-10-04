using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// The Unit for triggering a Return Event.
    /// </summary>
    [UnitCategory("Events/Community/Returns")]
    [RenamedFrom("Lasm.BoltExtensions.TriggerReturnEvent")]
    [RenamedFrom("Lasm.UAlive.TriggerReturnEvent")]
    [TypeIcon(typeof(BoltUnityEvent))]
    [RenamedFrom("Bolt.Addons.Community.ReturnEvents.TriggerReturnEvent")]
    public sealed class TriggerReturnEvent : GlobalEventUnit<ReturnEventArg>
    {
        /// <summary>
        /// Overrides the hook name that the Event Bus calls to decipher different event types.
        /// </summary>
        protected override string hookName => "TriggerReturn";

        [Serialize]
        private int _count;

        /// <summary>
        /// The amount of arguments to trigger with.
        /// </summary>
        [Inspectable]
        [UnitHeaderInspectable("Arguments")]
        public int count { get { return _count; } set { _count = Mathf.Clamp(value, 0, 10); } }

        /// <summary>
        /// Turns the event into a global event without a target.
        /// </summary>
        [UnitHeaderInspectable("Global")]
        public bool global;

        /// <summary>
        /// The value we store the returning value in, so we can return this to the result value port.
        /// </summary>
        public object storingValue;

        /// <summary>
        /// The Control Input to enter when we want to trigger the event.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter;

        /// <summary>
        /// The GameObject target that has the ReturnEventUnit.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        [NullMeansSelf]
        public ValueInput target;

        /// <summary>
        /// The name of the Return Event.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        [NullMeansSelf]
        public ValueInput name;

        /// <summary>
        /// The Control Output invoked immediately after we trigger the return. If the Return Event returns within the same frame, Complete will happen after this port is triggered.
        /// </summary>
        [DoNotSerialize]
        public ControlOutput exit;

        /// <summary>
        /// The Control Output port to invoke or run when the event has been returned from.
        /// </summary>
        [DoNotSerialize]
        public ControlOutput complete;

        /// <summary>
        /// The Value Inputs for the arguments.
        /// </summary>
        [DoNotSerialize]
        public List<ValueInput> arguments = new List<ValueInput>();

        /// <summary>
        /// The Value Ouput of the value we returned.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput value;
        
        /// <summary>
        /// Defines the ports of this unit.
        /// </summary>
        protected override void Definition()
        {
            base.Definition();

            arguments.Clear();

            enter = ControlInput("enter", Enter);

            name = ValueInput<string>("name", string.Empty);

            if (!global) target = ValueInput<GameObject>("target", (GameObject)null).NullMeansSelf();

            for (int i = 0; i < count; i++)
            {
                var input = ValueInput<object>(i.ToString());
                arguments.Add(input);
                Requirement(input, enter);
            }

            exit = ControlOutput("exit");
            value = ValueOutput<object>("value", GetValue);
            
            Succession(enter, exit);
            Requirement(name, enter);
            if (!global) Requirement(target, enter);
            Succession(enter, trigger);
        }

        /// <summary>
        /// Sets up the arguments for the Return Event, and triggers it upon entering the unit.
        /// </summary>
        public ControlOutput Enter(Flow flow)
        {
            List<object> argumentList = new List<object>();
            var eventData = new ReturnEventData(new ReturnEventArg(this, global ? (GameObject)null : flow.GetValue<GameObject>(target), flow.GetValue<string>(name), global, argumentList.ToArray()));
            argumentList.Add(eventData);
            argumentList.AddRange(arguments.Select(new System.Func<ValueInput, object>(flow.GetConvertedValue)));
            ReturnEvent.Trigger(this, global ? (GameObject)null : flow.GetValue<GameObject>(target), flow.GetValue<string>(name), global, argumentList.ToArray());
          
            return exit;
        }

        /// <summary>
        /// Returns the stored value to the output port.
        /// </summary>
        public object GetValue(Flow flow)
        {
            return storingValue;
        }
       
        /// <summary>
        /// Trigger the Trigger Return Event for returning to the trigger unit.
        /// </summary>
        public static void Trigger(ReturnEventArg args)
        {
            if (args.global) { EventBus.Trigger<ReturnEventArg>("TriggerReturn", args); return; }
            EventBus.Trigger<ReturnEventArg>("TriggerReturn", args);
        }

        /// <summary>
        /// Determines if we can return back to this unit.
        /// </summary>
        protected override bool ShouldTrigger(Flow flow, ReturnEventArg args)
        {
            if (args.trigger == this && global && args.name == flow.GetValue<string>(name)) return true;
            if (args.trigger == this && !global && args.target == flow.GetValue<GameObject>(target)) return true;
            return false;
        }
    }
}