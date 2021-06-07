using Unity.VisualScripting;
using Bolt.Addons.Community.Utility;

namespace Bolt.Addons.Community.Fundamentals.Editor
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