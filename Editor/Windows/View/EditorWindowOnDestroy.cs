namespace Unity.VisualScripting.Community
{
    [UnitSurtitle("Editor Window")]
    [UnitTitle("On Destroy")]
    public sealed class EditorWindowOnDestroy : EditorWindowEvent<EditorWindowEventArgs>
    {
        protected override string hookName => "EditorWindow_OnDestroy";

        protected override void AssignArguments(Flow flow, EditorWindowEventArgs args)
        {
            base.AssignArguments(flow, args);
            flow.SetValue(window, args.window);
        }
    }
}