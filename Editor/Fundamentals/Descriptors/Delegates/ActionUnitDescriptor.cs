using Bolt.Addons.Community.Fundamentals.Units.logic;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Fundamentals.Editor.Editor
{
    [Descriptor(typeof(ActionUnit))]
    public sealed class ActionUnitDescriptor : DelegateUnitDescriptor<ActionUnit>
    {
        public ActionUnitDescriptor(ActionUnit target) : base(target)
        {
        }

        protected override EditorTexture DefaultIcon()
        {
            return PathUtil.Load("action", CommunityEditorPath.Fundamentals);
        }

        protected override string DefaultName()
        {
            return "Action";
        }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("action", CommunityEditorPath.Fundamentals);
        }

        protected override string DefinedName()
        {
            return "Action";
        }
    }
}