namespace Unity.VisualScripting.Community
{
    [UnitSurtitle("Editor Window")]
    [UnitTitle("On Lost")]
    public sealed class EditorWindowOnLostFocus : EditorWindowEvent<EditorWindowEventArgs>
    {
        protected override string hookName => "EditorWindow_OnLostFocus";

        protected override void AssignArguments(Flow flow, EditorWindowEventArgs args)
        {
            base.AssignArguments(flow, args);
            flow.SetValue(window, args.window);
        }
    }
}