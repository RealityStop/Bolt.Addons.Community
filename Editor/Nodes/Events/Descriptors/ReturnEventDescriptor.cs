namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// A descriptor that assigns the ReturnEvents icon.
    /// </summary>
    [Descriptor(typeof(ReturnEvent))]
    public sealed class ReturnEventDescriptor : EventUnitDescriptor<ReturnEvent>
    {
        public ReturnEventDescriptor(ReturnEvent target) : base(target)
        {

        }

        protected override EditorTexture DefaultIcon()
        {
            return PathUtil.Load("return_event", CommunityEditorPath.Events);
        }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("return_event", CommunityEditorPath.Events);
        }
    }
}