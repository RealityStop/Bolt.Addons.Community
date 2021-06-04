using Unity.VisualScripting;
using Bolt.Addons.Community.Fundamentals.Units.logic;
using Bolt.Addons.Community.Utility;

namespace Bolt.Addons.Community.Fundamentals.Units.Utility.Editor
{
    [Editor(typeof(UnbindActionUnit))]
    public sealed class UnbindActionUnitEditor : UnbindDelegateUnitEditor<UnbindActionUnit, IAction>
    {
        public UnbindActionUnitEditor(Metadata metadata) : base(metadata)
        {
        }

        protected override string DefaultName => "Unbind";
    }
}