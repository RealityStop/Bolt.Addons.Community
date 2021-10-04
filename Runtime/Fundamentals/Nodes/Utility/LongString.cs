using System;

namespace Unity.VisualScripting.Community
{
    [TypeIcon(typeof(String))]
    [UnitTitle("Multiline String")]
    [UnitCategory("Community\\Utility")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.Units.Utility.MultilineStringUnit")]
    public class MultilineStringNode : Unit
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