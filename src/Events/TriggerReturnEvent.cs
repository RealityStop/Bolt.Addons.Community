using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ludiq;
using Bolt;
using System;
using System.Collections.ObjectModel;

namespace Bolt.Addons.Community.Events
{
    [UnitCategory("Events/Return")]
    public class TriggerReturnEvent : GameObjectEventUnit
    {
        [SerializeAs("count")]
        private int _count;

        [Inspectable]
        [UnitHeaderInspectable("Arguments")]
        public int count
        {
            get { return _count; }
            set { _count = Mathf.Clamp(value, 0, 10); }
        }

        [DoNotSerialize][PortLabelHidden]
        public ControlInput enter;

        [DoNotSerialize]
        public ControlOutput exit;

        [DoNotSerialize]
        public ControlOutput returned;

        [DoNotSerialize][PortLabelHidden]
        public ValueOutput @return;

        [DoNotSerialize][PortLabelHidden]
        public ValueInput name;

        [DoNotSerialize]
        public List<ValueInput> argumentPorts = new List<ValueInput>();

        public int hashId;

        public bool createdHash;

        public object returnValue;

        protected override bool ShouldTrigger()
        {
            throw InvalidArgumentCount(0);
        }

        protected override bool ShouldTrigger(object argument)
        {
            throw InvalidArgumentCount(1);
        }

        protected override bool ShouldTrigger(object[] arguments)
        {
            if (arguments.Length == 2) { throw InvalidArgumentCount(2); }
            else { return CompareNames(name, (string)arguments[1]); }
        }

        protected override void Definition()
        {
            enter = ControlInput("enter", new Action<Flow>(Enter))  ;
            
            base.Definition();

            ArgumentCount(count + 3);

            name = ValueInput<string>("name", string.Empty);

            argumentPorts.Clear();

            for (int i = 0; i < count; i++)
            {
                var _i = i;
                argumentPorts.Add(ValueInput<object>(_i.ToString()));
            }

            returned = ControlOutput("returned");

            @return = ValueOutput<object>("return", returnVal => GetReturn());
        }

        private void Enter(Flow flow)
        {
            flow.Invoke(trigger);

            List<object> args = new List<object>();
            args.Add(targetPort.GetValue<GameObject>());
            args.Add(name.GetValue<string>());

            if (!createdHash) {
                hashId = (new object()).GetHashCode();
                createdHash = true;
            }

            args.Add(this);

            foreach (ValueInput input in argumentPorts)
            {
                args.Add(input.GetValue<object>());
            }

            GameObjectEvent.Trigger<ReturnEvent>((GameObject)args[0], args.ToArray());
        }

        public void InvokeReturn()
        {
            Flow flow = Flow.New();
            flow.Invoke(returned);
        }

        private object GetReturn()
        {
            return returnValue;
        }
    }
}