using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.ReturnObjectNode")]
    [RenamedFrom("ReturnObjectNode")]
    [UnitCategory("Community\\ObjectPooling")]
    [UnitTitle("Return Object")]
    [UnitSurtitle("Object Pool")]
    public class ReturnObjectNode : Unit
    {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput Enter;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput Exit;

        [DoNotSerialize]
        [PortLabelHidden]
        [NullMeansSelf]
        public ValueInput Pool;

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput ObjectToReturn;

        protected override void Definition()
        {
            Enter = ControlInput(nameof(Enter), OnEnter);
            Exit = ControlOutput(nameof(Exit));

            Pool = ValueInput<ObjectPool>(nameof(Pool), null).NullMeansSelf();
            ObjectToReturn = ValueInput<GameObject>(nameof(ObjectToReturn), null);

            Succession(Enter, Exit);
        }

        private ControlOutput OnEnter(Flow flow)
        {
            var pool = flow.GetValue<ObjectPool>(Pool);
            var obj = flow.GetValue<GameObject>(ObjectToReturn);

            ObjectPool.ReturnObject(pool, obj);
            
            return Exit;
        }
    }
}