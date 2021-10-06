namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// The descriptor that sets the icon for Set Array Item.
    /// </summary>
    [Descriptor(typeof(SetArrayItem))]
    public sealed class SetArrayItemDescriptor : UnitDescriptor<SetArrayItem>
    {
        public SetArrayItemDescriptor(SetArrayItem unit) : base(unit)
        {

        }

        protected override EditorTexture DefaultIcon()
        {
            return PathUtil.Load("multi_array", CommunityEditorPath.Fundamentals);
        }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("multi_array", CommunityEditorPath.Fundamentals);
        }
    }
}
