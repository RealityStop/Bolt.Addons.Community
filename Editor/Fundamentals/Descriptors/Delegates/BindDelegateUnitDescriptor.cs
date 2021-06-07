using Bolt.Addons.Community.Utility;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Fundamentals.Editor
{
    public abstract class BindDelegateUnitDescriptor<T, TInterface> : UnitDescriptor<T>
        where T : BindDelegateUnit<TInterface>
        where TInterface : IDelegate
    {
        public BindDelegateUnitDescriptor(T target) : base(target)
        {
        }

        protected abstract string DefaultName();
        protected abstract string DefinedName();

        protected override string DefaultSurtitle()
        {
            return "Bind";
        }

        protected override string DefinedSurtitle()
        {
            return "Bind";
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