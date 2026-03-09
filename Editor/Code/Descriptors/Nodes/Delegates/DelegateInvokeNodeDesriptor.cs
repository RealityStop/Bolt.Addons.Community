namespace Unity.VisualScripting.Community
{
    public abstract class DelegateInvokeNodeDesriptor<T, TDelegate> : UnitDescriptor<T>
        where T : DelegateInvokeNode<TDelegate>
        where TDelegate : IDelegate
    {
        public DelegateInvokeNodeDesriptor(T target) : base(target)
        {
        }

        protected abstract string Prefix { get; }
        protected abstract string FallbackName { get; }

        protected override string DefinedTitle()
        {
            return target._delegate == null ? FallbackName : target._delegate.DisplayName;
        }


        protected override string DefinedShortTitle()
        {
            return target._delegate == null ? FallbackName : target._delegate.DisplayName;
        }


        protected override string DefaultTitle()
        {
            return target._delegate == null ? FallbackName : target._delegate.DisplayName;
        }


        protected override string DefaultShortTitle()
        {
            return target._delegate == null ? FallbackName : target._delegate.DisplayName;
        }

        protected override string DefaultSurtitle()
        {
            return Prefix;
        }

        protected override string DefinedSurtitle()
        {
            return Prefix;
        }
    }
}