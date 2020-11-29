using Bolt.Addons.Community.Fundamentals.Units.logic;
using Ludiq;

namespace Bolt.Addons.Community.Fundamentals.Editor.Editor
{
    public abstract class DelegateInvokeUnitDescriptor<T> : UnitDescriptor<T> where T : DelegateInvokeUnit
    {
        public DelegateInvokeUnitDescriptor(T target) : base(target)
        {
        }

        protected abstract string Prefix { get; }

        protected override string DefinedTitle()
        {
            return target._delegate == null ? Prefix : target._delegate.GetType().Name.Prettify();
        }


        protected override string DefinedShortTitle()
        {
            return target._delegate == null ? Prefix : target._delegate.GetType().Name.Prettify();
        }


        protected override string DefaultTitle()
        {
            return target._delegate == null ? Prefix : target._delegate.GetType().Name.Prettify();
        }


        protected override string DefaultShortTitle()
        {
            return target._delegate == null ? Prefix : target._delegate.GetType().Name.Prettify();
        }

        protected override string DefaultSubtitle()
        {
            return Prefix;
        }

        protected override string DefinedSubtitle()
        {
            return Prefix;
        }
    }
}