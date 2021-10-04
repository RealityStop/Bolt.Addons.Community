using System;

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(BindFuncNode))]
    public sealed class BindFuncNodeOption : BindDelegateNodeOption<BindFuncNode, IFunc>
    {
        [Obsolete()]
        public BindFuncNodeOption() : base() { }

        protected override string Label(bool human)
        {
            if (unit._delegate == null) return "Bind Func";
            return base.Label(human);
        }

        public BindFuncNodeOption(BindFuncNode unit) : base(unit)
        {
        }
    }
}