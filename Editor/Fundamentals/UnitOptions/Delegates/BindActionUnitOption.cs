using Bolt.Addons.Community.Fundamentals.Units.logic;
using Bolt.Addons.Community.Utility;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Variables.Editor.UnitOptions
{
    [FuzzyOption(typeof(BindActionUnit))]
    public sealed class BindActionUnitOption : BindDelegateUnitOption<BindActionUnit, IAction>
    {
        protected override string subCategory => "Actions";

        public BindActionUnitOption(BindActionUnit unit) : base(unit)
        {
        }
    }
}