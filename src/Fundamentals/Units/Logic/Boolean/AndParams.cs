using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Bolt.Addons.Community.Logic.Units
{
    [UnitShortTitle("And")]
    [UnitTitle("And (Params)")]
    [UnitCategory("Community\\Logic")]
    [TypeIcon(typeof(And))]
    public sealed class AndParam : Unit
    {
        public AndParam() { }

        
        [SerializeAs(nameof(argumentCount))]
        private int _argumentCount;

        [DoNotSerialize]
        public List<ValueInput> arguments { get; private set; }


        [DoNotSerialize]
        [Inspectable, UnitHeaderInspectable("Arguments")]
        public int argumentCount
        {
            get
            {
                return Mathf.Max(2,_argumentCount);
            }
            set
            {
                _argumentCount = Mathf.Clamp(value, 2, 10);
            }
        }

        [PortLabel("&&")]
        [DoNotSerialize]
        public ValueOutput output { get; private set; }

        protected override void Definition()
        {
            output = ValueOutput<bool>(nameof(output), GetValue);

            arguments = new List<ValueInput>();

            for (var i = 0; i < argumentCount; i++)
            {
                var argument = ValueInput<bool>("argument_" + i);
                arguments.Add(argument);
                Relation(argument, output);
            }
        }

        private bool GetValue(Recursion arg1)
        {
            foreach (var item in arguments)
            {
                if (!item.GetValue<bool>())
                    return false;
            }
            return true;
        }
    }
}
