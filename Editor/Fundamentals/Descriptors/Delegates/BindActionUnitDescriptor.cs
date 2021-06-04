using Bolt.Addons.Community.Fundamentals.Units.logic;
using Bolt.Addons.Community.Utility;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Fundamentals.Editor.Editor
{
    [Descriptor(typeof(BindActionUnit))]
    public sealed class BindActionUnitDescriptor : BindDelegateUnitDescriptor<BindActionUnit, IAction>
    {
        public BindActionUnitDescriptor(BindActionUnit target) : base(target)
        {
        }

        protected override EditorTexture DefaultIcon()
        {
            return PathUtil.Load("action_bind", CommunityEditorPath.Fundamentals);
        }

        protected override string DefaultName()
        {
            return "Bind";
        }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("action_bind", CommunityEditorPath.Fundamentals);
        }

        protected override string DefinedName()
        {
            return "Bind";
        }
    }
}