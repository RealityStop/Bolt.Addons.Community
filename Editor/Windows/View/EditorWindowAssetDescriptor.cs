using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility.Editor
{
    [Descriptor(typeof(EditorWindowAsset))]
    public sealed class EditorWindowAssetDescriptor : MacroDescriptor<EditorWindowAsset, MacroDescription>
    {
        public EditorWindowAssetDescriptor(EditorWindowAsset target) : base(target)
        {
        }
    }
}