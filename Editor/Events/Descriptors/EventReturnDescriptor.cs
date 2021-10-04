namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// A descriptor that assigns the EventReturns icon.
    /// </summary>
    [Descriptor(typeof(EventReturn))]
    public sealed class EventReturnDescriptor : UnitDescriptor<EventReturn>
    {
        public EventReturnDescriptor(EventReturn target) : base(target)
        {

        }

        protected override EditorTexture DefaultIcon()
        {
            return PathUtil.Load("return", CommunityEditorPath.Events);
        }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("return", CommunityEditorPath.Events);
        }
    }
}