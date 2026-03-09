using Unity.VisualScripting.Community.Utility;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(UnbindActionNode))]
    public sealed class UnbindActionNodeDescriptor : UnbindDelegateNodeDescriptor<UnbindActionNode, IAction>
    {
        public UnbindActionNodeDescriptor(UnbindActionNode target) : base(target)
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