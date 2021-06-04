using Bolt.Addons.Community.Fundamentals.Units.logic;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Fundamentals.Editor.Editor
{
    public abstract class DelegateInvokeUnitDescriptor<T> : UnitDescriptor<T> where T : DelegateInvokeUnit
    {
        public DelegateInvokeUnitDescriptor(T target) : base(target)
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