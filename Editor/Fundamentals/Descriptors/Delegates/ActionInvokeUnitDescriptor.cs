using Bolt.Addons.Community.Fundamentals.Units.logic;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Fundamentals.Editor.Editor
{
    [Descriptor(typeof(ActionInvokeUnit))]
    public sealed class ActionInvokeUnitDescriptor : DelegateInvokeUnitDescriptor<ActionInvokeUnit>
    {
        protected override string Prefix => "Invoke Action";

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