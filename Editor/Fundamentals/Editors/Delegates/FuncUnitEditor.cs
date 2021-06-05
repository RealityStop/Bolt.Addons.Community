using Unity.VisualScripting;
using Bolt.Addons.Community.Fundamentals.Units.logic;
using Bolt.Addons.Community.Utility;

namespace Bolt.Addons.Community.Fundamentals.Units.Utility.Editor
{
    [Editor(typeof(FuncUnit))]
    public sealed class FuncUnitEditor : DelegateUnitEditor<FuncUnit, IFunc>
    {
        public FuncUnitEditor(Metadata metadata) : base(metadata)
        {
        }

        protected override string DefaultName => "Func";
    }
}