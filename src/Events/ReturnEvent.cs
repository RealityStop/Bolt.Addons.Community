using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Ludiq;
using Bolt;
using System;
using System.Collections.ObjectModel;

namespace Bolt.Addons.Community.Events
{
    [UnitCategory("Events/Return")]
    public class ReturnEvent : GameObjectEventUnit
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

        [DoNotSerialize] [PortLabelHidden]
        public ValueInput name;

        [DoNotSerialize]
        public List<ValueOutput> argumentPorts = new List<ValueOutput>();

        public List<object> returnValues = new List<object>();

        public ValueOutput returnUnit;

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
            base.Definition();

            ArgumentCount(count + 3);
            
            name = ValueInput<string>("name", string.Empty);

            argumentPorts.Clear();

            returnUnit = ValueOutput<TriggerReturnEvent>("returnUnit", retUnit => ReturnUnit());

            for (int i = 0; i < count; i++)
            {
                var _i = i;

                // 0-2 are reserved for 0) return GameObject 1) return name 2) return identifier 
                // These are hidden from the visual eye, and combined into an AotDictionary
                argumentPorts.Add(ValueOutput<object>(i.ToString(), returnVal => arguments[_i + 3]));
            }
        }

        private TriggerReturnEvent ReturnUnit()
        {
            return (TriggerReturnEvent)arguments[2];
        }


    }
}
