using Bolt.Addons.Community.Fundamentals.Units.logic;
using Ludiq;

namespace Bolt.Addons.Community.Fundamentals.Editor.Editor
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
            return target._delegate == null ? DefinedName() : target._delegate.GetType().Name.Prettify();
        }


        protected override string DefinedShortTitle()
        {
            return target._delegate == null ? DefinedName() : target._delegate.GetType().Name.Prettify();
        }


        protected override string DefaultTitle()
        {
            return target._delegate == null ? DefaultName() : target._delegate.GetType().Name.Prettify();
        }


        protected override string DefaultShortTitle()
        {
            return target._delegate == null ? DefaultName() : target._delegate.GetType().Name.Prettify();
        }
    }
}