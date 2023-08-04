using Unity.VisualScripting;

namespace Bolt.Addons.Community.Fundamentals.Units.Utility
{
    [UnitCategory("Community\\Utility\\string")]
    [UnitTitle("IsEmptyOrWhitespace")]
    [TypeIcon(typeof(string))]
    public class StringNullorWhiteSpace : Unit
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

            if (string.IsNullOrEmpty(Value) || string.IsNullOrWhiteSpace(Value))
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
