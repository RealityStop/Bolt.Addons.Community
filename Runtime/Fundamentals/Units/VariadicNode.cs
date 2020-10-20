using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Bolt.Addons.Community.Fundamentals
{
    public abstract class VariadicNode<T> : Unit
    {
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
                return Mathf.Max(2, _argumentCount);
            }
            set
            {
                _argumentCount = Mathf.Clamp(value, 2, ArgumentLimit());
            }
        }

        protected abstract void BuildRelations(ValueInput arg);

        protected override void Definition()
        {
            arguments = new List<ValueInput>();

            for (var i = 0; i < Math.Min(argumentCount, ArgumentLimit()); i++)
            {
                var argument = ValueInput<T>("Arg_" + i);
                arguments.Add(argument);
                BuildRelations(argument);
            }
        }

        protected virtual int ArgumentLimit()
        {
            return 10;
        }
    }
}