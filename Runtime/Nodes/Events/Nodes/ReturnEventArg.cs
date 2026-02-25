using System;

namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// The arguments for matching events and the trigger.
    /// </summary>
    [RenamedFrom("Lasm.BoltExtensions.ReturnEventArg")]
    [RenamedFrom("Lasm.UAlive.ReturnEventArg")]
    [RenamedFrom("Bolt.Addons.Community.ReturnEvents.ReturnEventArg")]
    public struct ReturnEventArg
    {
        public readonly TriggerReturnEvent trigger;
        public readonly UnityEngine.GameObject target;
        public readonly bool global;
        public readonly object[] arguments;
        public readonly string name;
        public readonly Action<object> callback;
        public readonly bool isCallback;

        public ReturnEventArg(TriggerReturnEvent trigger, UnityEngine.GameObject target, string name, bool global, object[] arguments = null)
        {
            this.trigger = trigger;
            callback = null;
            isCallback = false;
            this.target = target;
            this.global = global;
            this.arguments = arguments;
            this.name = name;
        }

        public ReturnEventArg(Action<object> callback, UnityEngine.GameObject target, string name, bool global, object[] arguments = null)
        {
            this.trigger = null;
            this.callback = callback;
            isCallback = true;
            this.target = target;
            this.global = global;
            this.arguments = arguments;
            this.name = name;
        }

        public ReturnEventArg(ReturnEventData data)
        {
            trigger = data.args.trigger;
            callback = data.args.callback;
            isCallback = data.args.isCallback;
            target = data.args.target;
            global = data.args.global;
            arguments = data.args.arguments;
            name = data.args.name;
        }
    }
}