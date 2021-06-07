using Unity.VisualScripting;
using Bolt.Addons.Community.Utility;

namespace Bolt.Addons.Community.Fundamentals.Editor
{
    [Editor(typeof(ActionInvokeUnit))]
    public sealed class ActionInvokeUnitEditor : DelegateInvokeUnitEditor<ActionInvokeUnit, IAction>
    {
        public ActionInvokeUnitEditor(Metadata metadata) : base(metadata)
        {
        }

        protected override string DefaultName => "Action";
    }
}