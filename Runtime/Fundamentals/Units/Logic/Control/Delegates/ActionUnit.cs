using Bolt.Addons.Community.Utility;
using Unity.VisualScripting;
using System.Collections.Generic;

namespace Bolt.Addons.Community.Fundamentals.Units.logic
{
    [UnitCategory("Community/Control/Delegates")]
    [TypeIcon(typeof(Flow))]
    public sealed class ActionUnit : DelegateUnit
    {
        public IAction _action => _delegate as IAction;

        public ActionUnit() : base() { }

        public ActionUnit(IAction action) : base(action)
        {
        }

        protected override void InitializeDelegate(Flow flow)
        {
            _action.Initialize(flow, this, () => { flow.Invoke(invoke); });
        }
    }
}
