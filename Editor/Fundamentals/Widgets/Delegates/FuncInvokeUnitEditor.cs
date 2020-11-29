using Ludiq;
using Bolt.Addons.Community.Fundamentals.Units.logic;

namespace Bolt.Addons.Community.Fundamentals.Units.Utility.Editor
{
    [Editor(typeof(FuncInvokeUnit))]
    public sealed class FuncInvokeUnitEditor : DelegateInvokeUnitEditor
    {
        public FuncInvokeUnitEditor(Metadata metadata) : base(metadata)
        {
        }

        protected override string DefaultName => "Func";
    }
}