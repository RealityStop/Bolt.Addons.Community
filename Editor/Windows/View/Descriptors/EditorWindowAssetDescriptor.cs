namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(EditorWindowAsset))]
    public sealed class EditorWindowAssetDescriptor : MacroDescriptor<EditorWindowAsset, MacroDescription>
    {
        public EditorWindowAssetDescriptor(EditorWindowAsset target) : base(target)
        {
        }
    }
}