using Bolt.Addons.Community.Fundamentals.Units.logic;
using Ludiq;

namespace Bolt.Addons.Community.Fundamentals.Editor.Editor
{
    [Descriptor(typeof(ActionUnit))]
    public sealed class ActionUnitDescriptor : UnitDescriptor<ActionUnit>
    {
        public ActionUnitDescriptor(ActionUnit target) : base(target)
        {
        }

        protected override string DefinedTitle()
        {
            return target._action == null ? "Action" : target._action.GetType().Name.Prettify();
        }


        protected override string DefinedShortTitle()
        {
            return target._action == null ? "Action" : target._action.GetType().Name.Prettify();
        }


        protected override string DefaultTitle()
        {
            return target._action == null ? "Action" : target._action.GetType().Name.Prettify();
        }


        protected override string DefaultShortTitle()
        {
            return target._action == null ? "Action" : target._action.GetType().Name.Prettify();
        }
    }
}