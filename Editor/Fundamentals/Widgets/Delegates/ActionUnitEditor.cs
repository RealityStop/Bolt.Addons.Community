using Unity.VisualScripting;
using Bolt.Addons.Community.Fundamentals.Units.logic;
using Bolt.Addons.Community.Utility;

namespace Bolt.Addons.Community.Fundamentals.Units.Utility.Editor
{
    [Editor(typeof(ActionUnit))]
    public sealed class ActionUnitEditor : DelegateUnitEditor<IAction>
    {
        public ActionUnitEditor(Metadata metadata) : base(metadata)
        {
        }

        protected override string DefaultName => "Action";
    }
}