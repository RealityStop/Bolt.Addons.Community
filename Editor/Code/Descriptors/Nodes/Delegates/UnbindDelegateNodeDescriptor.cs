namespace Unity.VisualScripting.Community
{
    public abstract class UnbindDelegateNodeDescriptor<T, TInterface> : UnitDescriptor<T>
        where T : UnbindDelegateNode<TInterface>
        where TInterface : IDelegate
    {
        public UnbindDelegateNodeDescriptor(T target) : base(target)
        {
        }

        protected abstract string DefaultName();
        protected abstract string DefinedName();

        protected override string DefaultSurtitle()
        {
            return "Unbind";
        }

        protected override string DefinedSurtitle()
        {
            return "Unbind";
        }

        protected override string DefinedTitle()
        {
            return target._delegate == null ? DefinedName() : target._delegate.DisplayName;
        }


        protected override string DefinedShortTitle()
        {
            return target._delegate == null ? DefinedName() : target._delegate.DisplayName;
        }


        protected override string DefaultTitle()
        {
            return target._delegate == null ? DefaultName() : target._delegate.DisplayName;
        }


        protected override string DefaultShortTitle()
        {
            return target._delegate == null ? DefaultName() : target._delegate.DisplayName;
        }
    }
}