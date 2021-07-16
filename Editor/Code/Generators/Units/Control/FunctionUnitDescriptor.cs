using Unity.VisualScripting;

namespace Bolt.Addons.Community.Code
{
    [Descriptor(typeof(FunctionUnit))]
    public sealed class FunctionUnitDescriptor : UnitDescriptor<FunctionUnit>
    {
        public FunctionUnitDescriptor(FunctionUnit target) : base(target)
        {
        }

        protected override string DefinedShortTitle()
        {
            if (target.declaration != null) return string.IsNullOrEmpty(target.declaration.methodName) ? target.declaration.name : target.declaration.methodName;
            return base.DefaultShortTitle();
        }

        protected override string DefinedSubtitle()
        {
            if (target.declaration != null) return "Entry";
            return string.Empty;
        }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("flow_32", CommunityEditorPath.Code);
        }

        protected override EditorTexture DefaultIcon()
        {
            return PathUtil.Load("flow_32", CommunityEditorPath.Code);
        }
    }
}