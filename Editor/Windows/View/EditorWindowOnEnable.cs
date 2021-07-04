using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility.Editor
{
    [UnitSurtitle("Editor Window")]
    [UnitTitle("On Enable")]
    public sealed class EditorWindowOnEnable : EditorWindowEvent<EditorWindowEventArgs>
    {
        protected override string hookName => "EditorWindow_OnEnable";

        protected override void AssignArguments(Flow flow, EditorWindowEventArgs args)
        {
            base.AssignArguments(flow, args);
            flow.SetValue(window, args.window);
        }
    }
}