namespace Unity.VisualScripting.Community
{
    public abstract class WindowVariableNodeDescriptor : UnitDescriptor<WindowVariableNode>
    {
        public WindowVariableNodeDescriptor(WindowVariableNode unit) : base(unit)
        {

        }

        protected abstract string Prefix { get; }

        protected override string DefinedTitle()
        {
            return $"{Prefix} Window Variable";
        }

        protected override string DefinedShortTitle()
        {
            return $"{Prefix} Window Variable";
        }

        protected override string DefaultTitle()
        {
            return $"{Prefix} Window Variable";
        }

        protected override string DefaultShortTitle()
        {
            return $"{Prefix} Window Variable";
        }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("EditorWindow_Variable", CommunityEditorPath.Fundamentals);
        }

        protected override EditorTexture DefaultIcon()
        {
            return PathUtil.Load("EditorWindow_Variable", CommunityEditorPath.Fundamentals);
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
