using Bolt.Addons.Community.Utility;
using Unity.VisualScripting;
using UnityEngine;

namespace Bolt.Addons.Community.Fundamentals.Units.logic
{
    [UnitCategory("Community/Control")]
    [TypeIcon(typeof(Flow))]
    public class ActionInvokeUnit : DelegateInvokeUnit
    {
        public IAction _action => _delegate as IAction;

        public ActionInvokeUnit() : base() { }
        public ActionInvokeUnit(IAction action) : base(action) { }
    }
}
