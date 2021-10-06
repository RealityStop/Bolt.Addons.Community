using System;
using UnityObject = UnityEngine.Object;

namespace Unity.VisualScripting.Community.Deprecated
{
    /// <summary>
    /// Returns whether the value is within the given range (inclusive).
    /// </summary>
    [UnitCategory("Nulls")]
    [UnitTitle("Is Null")]
    [RenamedFrom("Bolt.Addons.Community.Logic.Units.IsNullValue")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.IsNullValue")]
    [TypeIcon(typeof(Null))]
    [Obsolete("This unit can be replaced by the built-in Equals (not Object.Equals, but the Bolt one in Logic)")]
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
            isNull = ValueOutput(nameof(isNull), (x) => IsNull(x));
            isNotNull = ValueOutput<bool>(nameof(isNotNull), (x) => !IsNull(x));

            Requirement(input, isNull);
            Requirement(input, isNotNull);
        }

        private bool IsNull(Flow flow)
        {
            var inputvalue = flow.GetValue(input);

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