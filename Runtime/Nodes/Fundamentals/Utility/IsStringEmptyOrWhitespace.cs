using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.StringNullorWhiteSpace")]
    [RenamedFrom("Unity.VisualScripting.Community.StringNullorWhiteSpace")]
    [UnitCategory("Community\\Utility\\string")]
    [UnitTitle("Is Empty Or Whitespace")]
    [TypeIcon(typeof(string))]
    public class IsStringEmptyOrWhitespace : Unit
    {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput Input;

        [DoNotSerialize]
        public ControlOutput True;

        [DoNotSerialize]
        public ControlOutput False;

        [PortLabelHidden]
        public ValueInput String;

        protected override void Definition()
        {
            Input = ControlInput(nameof(Input), Check);

            True = ControlOutput(nameof(True));
            False = ControlOutput(nameof(False));

            String = ValueInput<string>(nameof(String), default);

            Succession(Input, True);
            Succession(Input, False);
        }

        private ControlOutput Check(Flow flow)
        {
            string Value = flow.GetValue<string>(String);

            if (string.IsNullOrWhiteSpace(Value))
            {
                return True;
            }
            else
            {
                return False;
            }
        }

    }
}
