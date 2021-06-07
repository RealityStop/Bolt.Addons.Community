using Unity.VisualScripting;

namespace Bolt.Addons.Community.Fundamentals.Editor
{
    public abstract class DelegateUnitDescriptor<T> : UnitDescriptor<T> where T : DelegateUnit
    {
        public DelegateUnitDescriptor(T target) : base(target)
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