using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility.Editor
{
    [UnitCategory("Community/Editor")]
    [UnitTitle("Is Window")]
    [TypeIcon(typeof(GetWindowVariableUnit))]
    public sealed class WindowIsUnit : Unit
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