namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(CommentNode))]
    public class CommentDescriptor : UnitDescriptor<CommentNode>
    {
        public CommentDescriptor(CommentNode unit) : base(unit) { }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("comments", CommunityEditorPath.Fundamentals);
        }
    }
}