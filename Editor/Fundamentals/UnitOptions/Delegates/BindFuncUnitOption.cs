using Bolt.Addons.Community.Fundamentals.Units.logic;
using Bolt.Addons.Community.Utility;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Variables.Editor.UnitOptions
{
    [FuzzyOption(typeof(BindFuncUnit))]
    public sealed class BindFuncUnitOption : BindDelegateUnitOption<BindFuncUnit, IFunc>
    {
        protected override string subCategory => "Funcs";

        public BindFuncUnitOption(BindFuncUnit unit) : base(unit)
        {
        }
    }
}