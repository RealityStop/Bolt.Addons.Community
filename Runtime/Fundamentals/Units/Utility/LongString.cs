using System;
using Ludiq;

namespace Bolt.Addons.Community.Fundamentals.Units.Utility
{
    [TypeIcon(typeof(String))]
    [UnitTitle("Multiline String")]
    [UnitCategory("Community\\Utility")]
    public class MultilineStringUnit : Unit
    {
        [Inspectable]
        [UnitHeaderInspectable]
        [InspectorTextArea()]
        public string stringLiteral;

        /// <summary>
        /// The value of the provided string, which is a constant
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput output { get; private set; }

        protected override void Definition()
        {
            output = ValueOutput<string>(nameof(output), (flow) => stringLiteral);
        }
    }
}