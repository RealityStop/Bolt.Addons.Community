using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Addons.Community.Logic.Units
{
    /// <summary>
    /// Returns whether the value is within the given range (inclusive).
    /// </summary>
    [UnitCategory("Nulls")]
    [UnitTitle("Is Null")]
    [TypeIcon(typeof(Null))]
    public class IsNullValue : Unit
    {
        public IsNullValue() : base() { }

        /// <summary>
        /// The input value to check
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput input { get; private set; }


        /// <summary>
        /// The minimum value (inclusive)
        /// </summary>
        [DoNotSerialize]
        [PortLabel("Null")]
        public ValueOutput isNull { get; private set; }


        /// <summary>
        /// The maximum value (inclusive)
        /// </summary>
        [DoNotSerialize]
        [PortLabel("Not Null")]
        public ValueOutput isNotNull{ get; private set; }


        protected override void Definition()
        {
            input = ValueInput<object>(nameof(input)).AllowsNull();
            isNull = ValueOutput<bool>(nameof(isNull), (x) => input.GetValue() == null);
            isNotNull = ValueOutput<bool>(nameof(isNotNull), (x) => input.GetValue() != null);

            Relation(input, isNull);
            Relation(input, isNotNull);
        }
    }
}