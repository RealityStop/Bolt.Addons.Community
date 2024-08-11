using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.RandomOutputNode")]
    [UnitTitle("ChanceFlow")]
    [UnitCategory("Community\\Control")]
    [TypeIcon(typeof(SwitchOnInteger))]
    public class ChanceFlow : Unit
    {
        [DoNotSerialize]
        public ValueInput value;

        [DoNotSerialize]
        public ControlOutput trueOutput;

        [DoNotSerialize]
        public ControlInput enter;

        [DoNotSerialize]
        public ControlOutput falseOutput;

        protected override void Definition()
        {
            value = ValueInput("Probability", 0f);

            trueOutput = ControlOutput("True");
            falseOutput = ControlOutput("False");
            enter = ControlInput(nameof(enter), OnEnter);

            Succession(enter, trueOutput);
            Succession(enter, falseOutput);
        }

        public ControlOutput OnEnter(Flow flow)
        {
            float probability = flow.GetValue<float>(value);

            if (probability > 100f)
            {
                probability = 100f;
            }

            if (probability < 0f)
            {
                probability = 0f;
            }

            // Update the value input on the node with the clamped value
            value.SetDefaultValue(probability);

            // Clamp the probability value between 0 and 100
            probability = Mathf.Clamp(probability, 0f, 100f) / 100f;

            // Generate a random value between 0 and 1
            float randomValue = Random.value;

            // Check if the random value is less than or equal to the probability
            if (randomValue <= probability)
            {
                // Trigger the true output
                return trueOutput;
            }
            else
            {
                // Trigger the false output
                return falseOutput;
            }
        }
    }
}
