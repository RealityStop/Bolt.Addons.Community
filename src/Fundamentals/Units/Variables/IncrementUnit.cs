using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Bolt.Addons.Community.Fundamentals
{
    [UnitCategory("Variables")]
    [UnitShortTitle("Increment Variable")]
    [RenamedFrom("Bolt.Addons.Community.Variables.Units.IncrementUnit")]
    [UnitTitle("Increment")]
    public class IncrementUnit : VariableAdder
    {
        public IncrementUnit() : base() { }

        /// <summary>
        /// The value assigned to the variable before incrementing.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("pre")]
        public ValueOutput preIncrement { get; private set; }

        /// <summary>
        /// The value assigned to the variable after incrementing.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("post")]
        public ValueOutput postIncrement { get; private set; }

        protected override void Definition()
        {
            base.Definition();

            preIncrement = ValueOutput<float>(nameof(preIncrement), (x) => _preIncrementValue);
            postIncrement = ValueOutput<float>(nameof(postIncrement), (x) => _postIncrementValue);

            if (specifyFallback)
            {
                Requirement(fallback, preIncrement);
                Requirement(fallback, postIncrement);
            }
        }

        protected override float GetAmount(Flow flow)
        {
            return 1;
        }
    }
}