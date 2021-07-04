using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility.Editor
{
    [UnitSurtitle("Editor Window")]
    [UnitTitle("On Disable")]
    public sealed class EditorWindowOnDisable : EditorWindowEvent<EditorWindowEventArgs>
    {
        protected override string hookName => "EditorWindow_OnDisable";

        protected override void AssignArguments(Flow flow, EditorWindowEventArgs args)
        {
            base.AssignArguments(flow, args);
            flow.SetValue(window, args.window);
        }
    }
}