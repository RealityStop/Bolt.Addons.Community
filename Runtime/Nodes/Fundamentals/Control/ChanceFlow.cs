using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// Triggers an output based on a given probability (0 to 100%).
    /// </summary>
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.RandomOutputNode")]
    [UnitTitle("ChanceFlow")]
    [UnitCategory("Community\\Control")]
    [TypeIcon(typeof(SwitchOnInteger))]
    public class ChanceFlow : Unit
    {
        [DoNotSerialize]
        public ValueInput value;

        [DoNotSerialize]
        [PortLabel("Succeeded")]
        public ControlOutput trueOutput;

        [DoNotSerialize]
        public ControlInput enter;

        [DoNotSerialize]
        [PortLabel("Failed")]
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
            probability = Mathf.Clamp(probability, 0f, 100f) / 100f;
            float randomValue = Random.value;
            if (randomValue <= probability)
            {
                return trueOutput;
            }
            else
            {
                return falseOutput;
            }
        }
    }
}
