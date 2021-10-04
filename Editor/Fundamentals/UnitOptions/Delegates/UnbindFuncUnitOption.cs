using System;

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(UnbindFuncNode))]
    public sealed class UnbindFuncNodeOption : UnbindDelegateNodeOption<UnbindFuncNode, IFunc>
    {
        [Obsolete()]
        public UnbindFuncNodeOption() : base() { }

        protected override string Label(bool human)
        {
            if (unit._delegate == null) return "Unbind Func";
            return base.Label(human);
        }

        public UnbindFuncNodeOption(UnbindFuncNode unit) : base(unit)
        {
        }
    }
}