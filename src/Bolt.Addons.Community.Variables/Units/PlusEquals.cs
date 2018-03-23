using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Bolt.Addons.Community.Variables.Units
{
    [UnitCategory("Variables")]
    [UnitShortTitle("Plus Equals")]
    [UnitTitle("Plus Equals")]
    public sealed class PlusEquals : VariableAdder
    {
        public PlusEquals() : base() { }

        [DoNotSerialize]
        [PortLabel("amount")]
        public ValueInput amount { get; private set; }


        /// <summary>
        /// The value assigned to the variable after incrementing.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput postIncrement { get; private set; }


        protected override void Definition()
        {
            base.Definition();

            amount = ValueInput<float>(nameof(amount), 1);
            postIncrement = ValueOutput<float>(nameof(postIncrement), (x) => _postIncrementValue);
            
            Relation(name, amount);
            Relation(name, postIncrement);
            Relation(assign, amount);
            Relation(assign, postIncrement);
        }

        protected override float GetAmount()
        {
            return amount.GetValue<float>();
        }
    }
}