using System;

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(BindActionNode))]
    public sealed class BindActionNodeOption : BindDelegateNodeOption<BindActionNode, IAction>
    {
        [Obsolete()]
        public BindActionNodeOption() : base() { }

        protected override string Label(bool human)
        {
            if (unit._delegate == null) return "Bind Action";
            return base.Label(human);
        }

        public BindActionNodeOption(BindActionNode unit) : base(unit)
        {
        }
    }
}