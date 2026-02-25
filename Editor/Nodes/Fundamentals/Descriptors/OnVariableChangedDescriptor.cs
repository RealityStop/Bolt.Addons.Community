namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(OnVariableChanged))]
    public class OnVariableChangedDescriptor : UnitDescriptor<OnVariableChanged>
    {
        public OnVariableChangedDescriptor(OnVariableChanged unit) : base(unit) { }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("variableevent", CommunityEditorPath.Fundamentals);
        }
    }
}