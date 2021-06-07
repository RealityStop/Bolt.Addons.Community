using Bolt.Addons.Community.Utility;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Fundamentals.Editor
{
    [Descriptor(typeof(UnbindActionUnit))]
    public sealed class UnbindActionUnitDescriptor : UnbindDelegateUnitDescriptor<UnbindActionUnit, IAction>
    {
        public UnbindActionUnitDescriptor(UnbindActionUnit target) : base(target)
        {
        }

        protected override EditorTexture DefaultIcon()
        {
            return PathUtil.Load("action_unbind", CommunityEditorPath.Fundamentals);
        }

        protected override string DefaultName()
        {
            return "Unbind";
        }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("action_unbind", CommunityEditorPath.Fundamentals);
        }

        protected override string DefinedName()
        {
            return "Unbind";
        }
    }
}