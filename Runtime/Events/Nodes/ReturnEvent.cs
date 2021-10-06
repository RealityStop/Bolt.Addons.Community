using System.Collections.Generic;
using UnityEngine;
using System;

namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// The Event that starts some logic before return back flow and a value.
    /// </summary>
    [UnitCategory("Events/Community/Returns")]
    [RenamedFrom("Lasm.BoltExtensions.ReturnEvent")]
    [RenamedFrom("Lasm.UAlive.ReturnEvent")]
    [RenamedFrom("Bolt.Addons.Community.ReturnEvents.ReturnEvent")]
    public sealed class ReturnEvent : GlobalEventUnit<ReturnEventArg>
    {
        [Serialize]
        private int _count;

        /// <summary>
        /// The amount of arguments this event has.
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
        /// Outputs the data wrapper, which contains the references needed for returning back to the trigger.
        /// </summary>
        [DoNotSerialize]
        public ValueOutput eventData;

        /// <summary>
        /// A list of argument ports.
        /// </summary>
        [DoNotSerialize]
        public List<ValueOutput> arguments = new List<ValueOutput>();

        /// <summary>
        /// The name of the event.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput name;

        /// <summary>
        /// The target receiver GameObject for this event.
        /// </summary>
        [DoNotSerialize][PortLabelHidden][NullMeansSelf]
        public ValueInput target;

        /// <summary>
        /// Overrides the hook name that the Event Bus calls to decipher different event types.
        /// </summary>
        protected override string hookName { get { return "Return"; } }

        /// <summary>
        /// Defines the ports of this unit.
        /// </summary>
        protected override void Definition()
        {
            base.Definition();

            arguments.Clear();

            if (!global) target = ValueInput<GameObject>("target", (GameObject)null).NullMeansSelf();

            name = ValueInput<string>("name", string.Empty);

            eventData = ValueOutput<ReturnEventData>("data");

            for (int i = 0; i < count; i++)
            {
                arguments.Add(ValueOutput<object>(i.ToString()));
            }
        }

        /// <summary>
        /// Weither or not the event is able to trigger.
        /// </summary>
        protected override bool ShouldTrigger(Flow flow, ReturnEventArg args)
        {
            bool should = flow.GetValue<string>(name) == args.name;

            if ((args.isCallback ? arguments.Count == args.arguments.Length : arguments.Count + 1 == args.arguments.Length || (arguments.Count == 0 && args.arguments.Length == 0)) && should)
            {
                if (args.global)
                {
                    return true;
                }
                else
                {
                    if (args.target != null && args.target == flow.GetValue<GameObject>(target)) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Sets the values of all arguments when triggered.
        /// </summary>
        protected override void AssignArguments(Flow flow, ReturnEventArg args)
        {
            flow.SetValue(eventData, new ReturnEventData(args));

            for (int i = 0; i < arguments.Count; i++)
            {
                flow.SetValue(arguments[i], args.arguments[args.isCallback ? i : i+1]);
            }
        }

        /// <summary>
        /// Trigger the return event. This is meant for internal use of the trigger unit.
        /// </summary>
        /// <param name="trigger">The trigger unit we will return to when it hits a return unit.</param>
        /// <param name="target">The gameobject target of the event</param>
        /// <param name="name">The name of the event.</param>
        /// <param name="global">Is the event global to all Return Events? Will ignore the target GameObject. Target can be null in this case.</param>
        /// <param name="args">The arguments to send through.</param>
        public static void Trigger(TriggerReturnEvent trigger, GameObject target, string name, bool global = false, params object[] args)
        {
            EventBus.Trigger<ReturnEventArg>("Return", new ReturnEventArg(trigger, target, name, global, args));
        }

        public static void Trigger(GameObject target, string name, Action<object> callback = null, bool global = false, params object[] args)
        {
            EventBus.Trigger<ReturnEventArg>("Return", new ReturnEventArg(callback, target, name, global, args));
        }
    }
}