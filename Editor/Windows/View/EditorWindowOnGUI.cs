namespace Unity.VisualScripting.Community
{
    [UnitSurtitle("Editor Window")]
    [UnitTitle("On GUI")]
    public sealed class EditorWindowOnGUI : EditorWindowEvent<EditorWindowEventArgs>
    {
        protected override string hookName => "EditorWindow_OnGUI";

        protected override void AssignArguments(Flow flow, EditorWindowEventArgs args)
        {
            base.AssignArguments(flow, args);
            flow.SetValue(window, args.window);
        }
    }
}