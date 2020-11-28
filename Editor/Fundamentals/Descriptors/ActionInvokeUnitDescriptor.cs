using Bolt.Addons.Community.Fundamentals.Units.logic;
using Ludiq;

namespace Bolt.Addons.Community.Fundamentals.Editor.Editor
{
    [Descriptor(typeof(ActionInvokeUnit))]
    public sealed class ActionInvokeUnitDescriptor : UnitDescriptor<ActionInvokeUnit>
    {
        public ActionInvokeUnitDescriptor(ActionInvokeUnit target) : base(target)
        {
        }

        protected override string DefinedTitle()
        {
            return target._action == null ? "Invoke Action" : target._action.GetType().Name.Prettify();
        }


        protected override string DefinedShortTitle()
        {
            return target._action == null ? "Invoke Action" : target._action.GetType().Name.Prettify();
        }


        protected override string DefaultTitle()
        {
            return target._action == null ? "Invoke Action" : target._action.GetType().Name.Prettify();
        }


        protected override string DefaultShortTitle()
        {
            return target._action == null ? "Invoke Action" : target._action.GetType().Name.Prettify();
        }
    }
}