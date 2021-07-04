using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility.Editor
{
    [UnitCategory("Events/Community/Editor/Window")]
    public abstract class EditorWindowEvent<TArgs> : ManualEventUnit<TArgs>
    {
        [DoNotSerialize]
        public ValueOutput window;
        protected override void Definition()
        {
            base.Definition();

            window = ValueOutput<EditorWindowView>(nameof(window));
        }
    }
}