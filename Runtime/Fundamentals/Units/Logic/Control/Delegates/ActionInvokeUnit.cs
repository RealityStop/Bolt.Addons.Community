using System;
using System.Collections.Generic;
using Bolt.Addons.Community.Utility;
using Unity.VisualScripting;
using UnityEngine;

namespace Bolt.Addons.Community.Fundamentals.Units.logic
{
    [UnitCategory("Community/Control/Delegates")]
    [TypeIcon(typeof(Flow))]
    public sealed class ActionInvokeUnit : DelegateInvokeUnit<IAction>
    {
        protected override void Invoke(Flow flow, List<object> values)
        {
            flow.GetValue<IAction>(@delegate).Invoke(values.ToArray());
        }
    }
}
