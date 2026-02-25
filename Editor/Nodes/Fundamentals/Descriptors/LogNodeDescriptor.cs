namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(LogNode))]
    public class LogNodeDescriptor : UnitDescriptor<LogNode>
    {
        public LogNodeDescriptor(LogNode target) : base(target)
        {
        }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("debug", CommunityEditorPath.Fundamentals);
        }
    }
}
