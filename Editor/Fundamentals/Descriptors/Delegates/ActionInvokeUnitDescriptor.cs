using Bolt.Addons.Community.Utility;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Fundamentals.Editor
{
    [Descriptor(typeof(ActionInvokeUnit))]
    public sealed class ActionInvokeUnitDescriptor : DelegateInvokeUnitDescriptor<ActionInvokeUnit, IAction>
    {
        protected override string Prefix => "Invoke";
        protected override string FallbackName => "Action";

        public ActionInvokeUnitDescriptor(ActionInvokeUnit target) : base(target)
        {
        }

        protected override EditorTexture DefaultIcon()
        {
            return PathUtil.Load("action_invoke", CommunityEditorPath.Fundamentals);
        }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("action_invoke", CommunityEditorPath.Fundamentals);
        }
    }
}