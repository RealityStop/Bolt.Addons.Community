using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("SizeString")]
    [UnitTitle("Size")]
    [UnitCategory("Community\\Utility\\string")]
    [TypeIcon(typeof(string))]
    public class SizeString : Unit
    {
        [UnitHeaderInspectable("Size")]
        [Inspectable]
        public int size;

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput Value;
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput Result;
        protected override void Definition()
        {
            Value = ValueInput<string>(nameof(Value), default);
            Result = ValueOutput<string>(nameof(Result), Enter_);
        }

        public string Enter_(Flow flow)
        {
            var value = flow.GetValue(Value);

            var NewValue = $"<size={size}>" + value + "</size>";

            return NewValue;
        }
    }

}