using System;

namespace Unity.VisualScripting.Community
{
    [UnitTitle("As")]
    [UnitCategory("Community/Utility")]
    public class AsUnit : Unit
    {
        [UnitHeaderInspectable("Type")]
        [Inspectable]
        [InspectorLabel("Type")]
        [InspectorWide(true)]
        public Type AsType = typeof(float);
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput Value;
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput Result;

        public override bool canDefine => AsType != null;

        protected override void Definition()
        {
            Value = ValueInput<object>(nameof(Value));
            Result = ValueOutput(AsType, nameof(Result), (flow) =>
            {
                var _value = flow.GetValue(Value);
                return _value.ConvertTo(AsType);
            });
        }
    }
}
