using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility.Editor
{
    [Descriptor(typeof(EditorWindowView))]
    public sealed class EditorWindowViewDescriptor : MacroDescriptor<EditorWindowView, MacroDescription>
    {
        public EditorWindowViewDescriptor(EditorWindowView target) : base(target)
        {
        }
    }
}