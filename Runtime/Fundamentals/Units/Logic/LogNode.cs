using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [UnitTitle("Log")]
    [UnitCategory("Community\\Logic")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.Units.logic.LogUnit")]
    public class LogNode : Unit
    {
        public const int ArgumentLimit = 10;

        public LogNode() : base() { }

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput format { get; private set; }


        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput input { get; private set; }


        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput output { get; private set; }


        [SerializeAs(nameof(argumentCount))]
        private int _argumentCount;


        [DoNotSerialize]
        public List<ValueInput> arguments { get; protected set; }

        [DoNotSerialize]
        [Inspectable, UnitHeaderInspectable("Arguments")]
        public int argumentCount
        {
            get
            {
                return Mathf.Max(0, _argumentCount);
            }
            set
            {
                _argumentCount = Mathf.Clamp(value, 0, ArgumentLimit);
            }
        }

        protected override void Definition()
        {
            format = ValueInput<string>(nameof(format), "");

            input = ControlInput(nameof(input), (flow) => Log(flow));
            output = ControlOutput(nameof(output));

            arguments = new List<ValueInput>();
            for (var i = 0; i < Math.Min(argumentCount, ArgumentLimit); i++)
            {
                var argument = ValueInput<object>("Arg_" + i);
                arguments.Add(argument);
                Requirement(argument, input);
            }

            Succession(input, output);
            Requirement(format, input);
        }

        private ControlOutput Log(Flow flow)
        {
            string formatstr = flow.GetValue<string>(format);

            //Optimized check for 1 arg and no format.
            if (argumentCount == 1 && string.IsNullOrEmpty(formatstr))
            {
                Debug.Log(flow.GetValue(arguments[0]).ToString());
                return output;
            }


            var stringArgs = arguments.Select<ValueInput, string>(x =>
            {

                var val = flow.GetValue(x);
                if (val is string)
                    return val as string;
                return val.ToString();
            });

            Debug.Log(string.Format(flow.GetValue<string>(format), stringArgs.ToArray()));
            return output;
        }
    }
}