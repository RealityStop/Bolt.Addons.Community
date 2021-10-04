using System;

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(UnbindActionNode))]
    public sealed class UnbindActionNodeOption : UnbindDelegateNodeOption<UnbindActionNode, IAction>
    {
        [Obsolete()]
        public UnbindActionNodeOption() : base() { }

        protected override string Label(bool human)
        {
            if (unit._delegate == null) return "Unbind Action";
            return base.Label(human);
        }

        public UnbindActionNodeOption(UnbindActionNode unit) : base(unit)
        {
        }
    }
}