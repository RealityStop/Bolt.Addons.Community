using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility.Editor
{
    [UnitCategory("Community/Editor")]
    [UnitTitle("Get Window Variable")]
    public sealed class GetWindowVariableUnit : EditorWindowVariableUnit
    {
        [DoNotSerialize]
        public ValueOutput value;

        protected override void Definition()
        {
            base.Definition();
            value = ValueOutput<object>(nameof(value), (flow) =>
            {
                return flow.GetValue<EditorWindowView>(target).variables.Get(flow.GetValue<string>(name));
            });
        }
    }
}