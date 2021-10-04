using System.Collections.Generic;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community
{
    [UnitCategory("Community/Control/Delegates")]
    [TypeIcon(typeof(Flow))]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.ActionInvokeUnit")]
    public sealed class ActionInvokeNode : DelegateInvokeNode<IAction>
    {
        protected override void Invoke(Flow flow, List<object> values)
        {
            flow.GetValue<IAction>(@delegate).Invoke(values.ToArray());
        }
    }
}
