namespace Unity.VisualScripting.Community
{
    [UnitCategory("Community\\Documentation")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.Units.Documenting.SomeValue")]
    public class SomeValue : Unit
    {
        [Inspectable]
        public bool IsInteger;

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput output { get; private set; }

        protected override void Definition()
        {
            if (IsInteger)
                output = ValueOutput<int>(nameof(output), (flow)=> UnityEngine.Random.Range(0,1));
            else
                output = ValueOutput<float>(nameof(output), (flow) => UnityEngine.Random.Range(0.0f, 1.0f));
        }
    }
}