namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(AsUnit))]
    public class AsTypeDescriptor : UnitDescriptor<AsUnit>
    {
        public AsTypeDescriptor(AsUnit target) : base(target)
        {
        }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("as_32", CommunityEditorPath.Code);
        }
    }
}
