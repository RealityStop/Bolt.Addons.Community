using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility.Editor
{
    public abstract class EditorWindowVariableUnitDescriptor : UnitDescriptor<EditorWindowVariableUnit>
    {
        public EditorWindowVariableUnitDescriptor(EditorWindowVariableUnit unit) : base(unit)
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
    }
}
