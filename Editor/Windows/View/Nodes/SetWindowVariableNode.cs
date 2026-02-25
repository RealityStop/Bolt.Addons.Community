namespace Unity.VisualScripting.Community
{
    [UnitCategory("Community/Editor")]
    [UnitTitle("Set Window Variable")]
    public sealed class SetWindowVariableNode : WindowVariableNode
    {
        [DoNotSerialize]
        public ControlInput enter;
        [DoNotSerialize]
        public ControlOutput exit;
        [DoNotSerialize]
        public ValueInput value;

        protected override void Definition()
        {
            base.Definition();
            enter = ControlInput("enter", (flow) =>
            {
                flow.GetValue<EditorWindowView>(target).variables.Set(flow.GetValue<string>(name), flow.GetValue(value));
                return exit;
            });
            exit = ControlOutput("exit");

            value = ValueInput<object>("value");
        }
    }
}