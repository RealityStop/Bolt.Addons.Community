namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(Todo))]
    public class TodoDescriptor : UnitDescriptor<Todo>
    {
        public TodoDescriptor(Todo unit) : base(unit) { }

        protected override EditorTexture DefinedIcon()
        {
            if (unit.ErrorIfHit)
                PathUtil.Load("construction_alarm", CommunityEditorPath.Fundamentals);
            return PathUtil.Load("construction", CommunityEditorPath.Fundamentals);
        }
    }
}