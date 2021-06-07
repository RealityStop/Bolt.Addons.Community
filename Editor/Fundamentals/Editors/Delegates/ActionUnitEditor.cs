using Unity.VisualScripting;
using Bolt.Addons.Community.Utility;

namespace Bolt.Addons.Community.Fundamentals.Editor
{
    [Editor(typeof(ActionUnit))]
    public sealed class ActionUnitEditor : DelegateUnitEditor<ActionUnit, IAction>
    {
        public ActionUnitEditor(Metadata metadata) : base(metadata)
        {
        }

        protected override string DefaultName => "Action";
    }
}