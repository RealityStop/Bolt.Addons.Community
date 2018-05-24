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
    [UnitCategory("Logic")]
    [TypeIcon(typeof(Bolt.And))]
    public class Between : Unit
    {
        public Between() : base() { }

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
        public ValueInput min { get; private set; }


        /// <summary>
        /// The maximum value (inclusive)
        /// </summary>
        [DoNotSerialize]
        public ValueInput max { get; private set; }



        /// <summary>
        /// Whether the input is within the specified range.
        /// </summary>
        [DoNotSerialize]
        public ValueOutput within { get; private set; }


        protected override void Definition()
        {
            input = ValueInput<float>(nameof(input));
            min = ValueInput<float>(nameof(min), 0);
            max = ValueInput<float>(nameof(max), 0);
            within = ValueOutput<bool>(nameof(within), (x)=>GetWithin());

            Relation(input, within);
            Relation(min, within);
            Relation(max, within);
        }

        private bool GetWithin()
        {
            float inputValue = input.GetValue<float>();

            return (inputValue >= min.GetValue<float>()) &&
                    (inputValue <= max.GetValue<float>());
        }
    }
}