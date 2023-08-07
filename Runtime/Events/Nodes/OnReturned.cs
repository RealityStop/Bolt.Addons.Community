using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Bolt.Addons.Community.Fundamentals.ObjectPooling
{
    [UnitCategory("Events/Community/ObjectPooling")]
    [UnitTitle("OnReturned")]
    public class OnReturned : EventUnit<PoolData>
    {
        protected override bool register => true;

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput Pool;

        [DoNotSerialize]
        public ValueOutput Result;

        public override EventHook GetHook(GraphReference reference)
        {
            return ObjectPoolEvents.OnReturned;
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