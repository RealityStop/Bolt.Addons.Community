using System;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Lasm.BoltAddons.UnitTools.Events
{
    [UnitSurtitle("Local Machine")]
    [UnitShortTitle("Trigger")]
    [UnitTitle("Trigger Local Machine Event")]
    [UnitCategory("Events/Machine")]
    public class TriggerLocalMachineEvent : Unit
    {
        [DoNotSerialize]
        [Inspectable]
        [UnitHeaderInspectable]
        public TriggerType trigger;

        [DoNotSerialize]
        public ControlInput enter;
        [DoNotSerialize]
        public ControlOutput exit;

        [DoNotSerialize]
        public ValueInput name;

        [DoNotSerialize]
        public ValueInput count;

        [SerializeAs("argumentCount")]
        private int _argumentCount;

        private List<ValueInput> arguments;

        [DoNotSerialize, Inspectable, UnitHeaderInspectable("Arguments")]
        public int argumentCount
        {
            get
            {
                return this._argumentCount;
            }
            set
            {
                this._argumentCount = Mathf.Clamp(value, 0, 10);
            }
        }

        protected override void Definition()
        {
            enter = ControlInput("enter", new Action<Flow>(Enter));

            name = ValueInput<string>("name", string.Empty);

            if (trigger == TriggerType.Children)
            {
                count = ValueInput<int>("below", 0);
            } else if (trigger == TriggerType.Parent)
            {
                count = ValueInput<int>("above", 0);
            } else
            {
                if (trigger == TriggerType.Both)
                {
                    count = ValueInput("away", 0);
                }
            }

            for (int i = 0; i < argumentCount; i++)
            {
                int _i = i;
                base.ValueOutput<object>("argument_" + i, (Recursion recursion) => arguments[1 + _i]);
            }

            exit = ControlOutput("exit");

            Relation(enter, exit);
        }
        
        private void Enter(Flow flow)
        {

        }
    }

    public enum TriggerType
    {
        Parent,
        Children,
        Both,
        Self
    }
}
