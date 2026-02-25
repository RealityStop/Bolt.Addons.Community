using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    public abstract class DelegateNodeDescriptor<T> : UnitDescriptor<T> where T : DelegateNode
    {
        public DelegateNodeDescriptor(T target) : base(target)
        {
        }

        protected abstract string DefaultName();
        protected abstract string DefinedName();

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