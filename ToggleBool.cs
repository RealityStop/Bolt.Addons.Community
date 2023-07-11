using Unity.VisualScripting;

namespace Bolt.Addons.Community.Fundamentals.Units.Utility
{
    [UnitCategory("Community\\Utility")]
    [UnitTitle("Toggle Boolean")]
    [TypeIcon(typeof(ToggleFlow))]
    public class ToggleBool : Unit
    {
        [DoNotSerialize]
        public ValueInput Value;

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput Result;

        private bool Cached = false;

        private bool value;

        protected override void Definition()
        {
            Value = ValueInput<bool>(nameof(Value));
            Result = ValueOutput(nameof(Result), GetResult);
        }

        private bool GetResult(Flow flow)
        {
            if (!Cached)
            {
                value = (bool)flow.GetValue(Value);

                value = !value;

                Cached = true;

                return value;
            }
            else
            {
                flow.SetValue(Value, value);

                value = !value;

                return value;
            }


        }
    }

}