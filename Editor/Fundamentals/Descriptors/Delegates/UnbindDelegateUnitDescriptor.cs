using Bolt.Addons.Community.Utility;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Fundamentals.Editor
{
    public abstract class UnbindDelegateUnitDescriptor<T, TInterface> : UnitDescriptor<T>
        where T : UnbindDelegateUnit<TInterface>
        where TInterface : IDelegate
    {
        public UnbindDelegateUnitDescriptor(T target) : base(target)
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