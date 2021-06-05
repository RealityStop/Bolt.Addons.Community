using Unity.VisualScripting;
using Bolt.Addons.Community.Fundamentals.Units.logic;
using Bolt.Addons.Community.Utility;

namespace Bolt.Addons.Community.Fundamentals.Units.Utility.Editor
{
    [Editor(typeof(UnbindFuncUnit))]
    public sealed class UnbindFuncUnitEditor : DelegateUnitEditor<IFunc>
    {
        public UnbindFuncUnitEditor(Metadata metadata) : base(metadata)
        {
        }

        protected override string DefaultName => "Unbind";
    }
}