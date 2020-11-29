using Bolt.Addons.Community.Utility;
using Ludiq;
using System;
using System.Collections.Generic;

namespace Bolt.Addons.Community.Fundamentals.Units.logic
{
    public class ActionUnit : DelegateUnit
    {
        public IAction _action => _delegate as IAction;

        public ActionUnit() : base() { }

        public ActionUnit(IDelegate @delegate)
        {
        }

        protected override void InitializeDelegate(Flow flow)
        {
            _action.Initialize(flow, this, () => { flow.Invoke(invoke); });
        }
    }
}
