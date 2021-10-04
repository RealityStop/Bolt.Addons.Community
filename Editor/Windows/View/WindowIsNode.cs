using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [UnitCategory("Community/Editor")]
    [UnitTitle("Is Window")]
    [TypeIcon(typeof(GetWindowVariableNode))]
    public sealed class WindowIsNode : Unit
    {
        [UnitHeaderInspectable]
        public EditorWindowAsset asset;
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput target;
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput result;

        protected override void Definition()
        {
            target = ValueInput<EditorWindowView>("target");
            result = ValueOutput("result", (flow) =>
            {
                return flow.GetValue<EditorWindowView>(target).asset == asset;
            });
        }
    }
}