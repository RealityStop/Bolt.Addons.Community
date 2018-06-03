using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityObject = UnityEngine.Object;

namespace Bolt.Addons.Community.Fundamentals
{
    /// <summary>
    /// Returns whether the value is within the given range (inclusive).
    /// </summary>
    [UnitCategory("Nulls")]
    [UnitTitle("Is Null")]
    [RenamedFrom("Bolt.Addons.Community.Logic.Units.IsNullValue")]
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
        public ValueOutput isNotNull { get; private set; }


        protected override void Definition()
        {
            input = ValueInput<object>(nameof(input)).AllowsNull();
            isNull = ValueOutput(nameof(isNull), (x) => IsNull());
            isNotNull = ValueOutput<bool>(nameof(isNotNull), (x) => !IsNull());

            Relation(input, isNull);
            Relation(input, isNotNull);
        }

        private bool IsNull()
        {
            var inputvalue = input.GetValue();

            if (inputvalue is UnityObject)
            {
                // Required cast because of Unity's custom == operator.
                return (UnityObject)inputvalue == null;
            }
            else
            {
                return inputvalue == null;
            }
        }
    }
}