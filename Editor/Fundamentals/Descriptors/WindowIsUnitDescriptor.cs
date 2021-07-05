using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility.Editor
{
    public sealed class WindowIsUnitDescriptor : UnitDescriptor<WindowIsUnit>
    {
        public WindowIsUnitDescriptor(WindowIsUnit unit) : base(unit)
        {

        }

        protected override string DefinedTitle()
        {
            return "Window Is";
        }

        protected override string DefinedShortTitle()
        {
            return "Window Is";
        }

        protected override string DefaultTitle()
        {
            return "Window Is";
        }

        protected override string DefaultShortTitle()
        {
            return "Window Is";
        }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("EditorWindow", CommunityEditorPath.Fundamentals);
        }

        protected override EditorTexture DefaultIcon()
        {
            return PathUtil.Load("EditorWindow", CommunityEditorPath.Fundamentals);
        }
    }
}
