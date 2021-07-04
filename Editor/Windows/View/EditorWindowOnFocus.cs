using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility.Editor
{
    [UnitSurtitle("Editor Window")]
    [UnitTitle("On Focus")]
    public sealed class EditorWindowOnFocus : EditorWindowEvent<EditorWindowEventArgs>
    {
        protected override string hookName => "EditorWindow_OnFocus";

        protected override void AssignArguments(Flow flow, EditorWindowEventArgs args)
        {
            base.AssignArguments(flow, args);
            flow.SetValue(window, args.window);
        }
    }
}