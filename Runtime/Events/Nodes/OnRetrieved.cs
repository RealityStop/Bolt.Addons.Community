using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.OnRetrieved")]
    [RenamedFrom("OnRetrieved")]
    [UnitCategory("Events/Community/ObjectPooling")]
    [UnitSurtitle("Object Pool")]
    [UnitTitle("On Retrieved")]
    public class OnRetrieved : EventUnit<PoolData>
    {
        protected override bool register => true;

        [DoNotSerialize]
        [PortLabelHidden]
        [NullMeansSelf]
        public ValueInput Pool;

        [DoNotSerialize]
        [PortLabel("Obj")]
        public ValueOutput Result;

        public override EventHook GetHook(GraphReference reference)
        {
            return CommunityEvents.OnRetrieved;
        }

        protected override void Definition()
        {
            base.Definition();

            Pool = ValueInput<ObjectPool>(nameof(Pool), default).NullMeansSelf();
            Result = ValueOutput<GameObject>(nameof(Result));
        }

        protected override bool ShouldTrigger(Flow flow, PoolData args)
        {
            if (args.pool == flow.GetValue<ObjectPool>(Pool))
            {
                flow.SetValue(Result, args.arg);
                return true;
            }
            return false;
        }
    }
}
