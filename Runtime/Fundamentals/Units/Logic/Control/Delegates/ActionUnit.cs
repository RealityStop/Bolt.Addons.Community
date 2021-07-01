using Bolt.Addons.Community.Utility;
using Unity.VisualScripting;
using UnityEngine;

namespace Bolt.Addons.Community.Fundamentals
{
    [UnitCategory("Community/Control/Delegates")]
    [TypeIcon(typeof(Flow))]
    public sealed class ActionUnit : DelegateUnit
    {
        public IAction _action => _delegate as IAction;

        public ActionUnit() : base() { }
        public ActionUnit(IDelegate del) : base(del)
        {

        }

        protected override void InitializeDelegate(Flow flow, bool instance = false)
        {
            if (instance)
            {
                _action.SetInstance(flow, this, () => { var _flow = Flow.New(flow.stack.ToReference()); _flow.Invoke(invoke); });
                return;
            }
            _action.Initialize(flow, this, () => { var _flow = Flow.New(flow.stack.ToReference()); _flow.Invoke(invoke); });
        }
    }
}
