using Unity.VisualScripting;
using Bolt.Addons.Community.Utility;

namespace Bolt.Addons.Community.Fundamentals.Editor
{
    [Editor(typeof(BindFuncUnit))]
    public sealed class BindFuncUnitEditor : BindDelegateUnitEditor<BindFuncUnit, IFunc>
    {
        public BindFuncUnitEditor(Metadata metadata) : base(metadata)
        {
        }

        protected override string DefaultName => "Bind";
    }
}