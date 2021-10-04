namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(EditorWindowView))]
    public sealed class EditorWindowViewDescriptor : MacroDescriptor<EditorWindowView, MacroDescription>
    {
        public EditorWindowViewDescriptor(EditorWindowView target) : base(target)
        {
        }
    }
}