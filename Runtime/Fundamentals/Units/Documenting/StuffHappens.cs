namespace Unity.VisualScripting.Community
{
    [UnitCategory("Community\\Documentation")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.Units.Documenting.StuffHappens")]
    public class StuffHappens : Unit
    {

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter { get; private set; }


        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), (x) => exit);
            exit = ControlOutput(nameof(exit));

            Succession(enter, exit);
        }
    }
}