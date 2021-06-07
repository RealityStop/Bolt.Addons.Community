using Unity.VisualScripting;
using Bolt.Addons.Community.Utility;

namespace Bolt.Addons.Community.Fundamentals.Editor
{
    [Editor(typeof(BindActionUnit))]
    public sealed class BindActionUnitEditor : BindDelegateUnitEditor<BindActionUnit, IAction>
    {
        public BindActionUnitEditor(Metadata metadata) : base(metadata)
        {
        }

        protected override string DefaultName => "Bind";
    }
}