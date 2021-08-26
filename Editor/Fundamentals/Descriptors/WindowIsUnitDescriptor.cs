using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility.Editor
{
    [Descriptor(typeof(WindowIsUnit))]
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
            return PathUtil.Load("WindowIs", CommunityEditorPath.Fundamentals);
        }

        protected override EditorTexture DefaultIcon()
        {
            return PathUtil.Load("WindowIs", CommunityEditorPath.Fundamentals);
        }

        protected override void DefinedPort(IUnitPort port, UnitPortDescription description)
        {
            base.DefinedPort(port, description);

#if !VISUAL_SCRIPTING_1_7_3
            if (port == unit.target)
            {
                description.icon = PathUtil.Load("EditorWindow", CommunityEditorPath.Fundamentals);
            }
#endif
        }
    }
}
