using Unity.VisualScripting;
using UnityEngine;

namespace Bolt.Addons.Community.Fundamentals.ObjectPooling
{
    [UnitCategory("Events/Community/ObjectPooling")]
    public class OnRetrieved : EventUnit<PoolData>
    {
        protected override bool register => true;

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput Pool;

        [DoNotSerialize]
        public ValueOutput Result;

        public override EventHook GetHook(GraphReference reference)
        {
            return ObjectPoolEvents.OnRetrieved;
        }

        protected override void Definition()
        {
            base.Definition();

            Pool = ValueInput<CustomObjectPool>(nameof(Pool), default);
            Result = ValueOutput<GameObject>(nameof(Result));
        }

        protected override bool ShouldTrigger(Flow flow, PoolData args)
        {
            if (args.pool == flow.GetValue<CustomObjectPool>(Pool))
            {
                flow.SetValue(Result, args.arg);
                return true;
            }
            return false;
        }
    }
}
