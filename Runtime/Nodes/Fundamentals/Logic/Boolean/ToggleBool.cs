using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.ToggleBool")]
    [UnitCategory("Community\\Utility")]
    [UnitTitle("Toggle Boolean")]
    [TypeIcon(typeof(ToggleFlow))]
    public class ToggleBool : Unit, IGraphElementWithData
    {
        private class Data : IGraphElementData
        {
            public bool Cached = false;

            public bool value;
        }

        [DoNotSerialize]
        public ValueInput Value;

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput Result;

        protected override void Definition()
        {
            Value = ValueInput<bool>(nameof(Value));
            Result = ValueOutput(nameof(Result), GetResult);
        }

        private bool GetResult(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);
            if (!data.Cached)
            {
                data.value = (bool)flow.GetValue(Value);

                data.value = !data.value;

                data.Cached = true;

                return data.value;
            }
            else
            {
                flow.SetValue(Value, data.value);

                data.value = !data.value;

                return data.value;
            }
        }

        public IGraphElementData CreateData()
        {
            return new Data();
        }
    }

}
