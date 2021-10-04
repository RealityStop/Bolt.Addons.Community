namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// The descriptor that sets the icon for CreateMultiArray.
    /// </summary>
    [Descriptor(typeof(CreateArray))]
    public class CreateArrayDescriptor : UnitDescriptor<CreateArray>
    {
        public CreateArrayDescriptor(CreateArray unit) : base(unit)
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