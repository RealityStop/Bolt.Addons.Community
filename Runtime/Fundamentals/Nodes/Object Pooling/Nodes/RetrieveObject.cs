using UnityEngine;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.RetrieveObjectNode")]
    [RenamedFrom("RetrieveObjectNode")]
    [UnitCategory("Community\\ObjectPooling")]
    [UnitTitle("Retrieve Object")]
    [UnitSurtitle("Object Pool")]
    public class RetrieveObjectNode : Unit
    {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput Enter;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput Retrieved;

        [DoNotSerialize]
        [PortLabelHidden]
        [NullMeansSelf]
        public ValueInput Pool;

        [DoNotSerialize]
        [PortLabel("Obj")]
        public ValueOutput Result;

        protected override void Definition()
        {
            Enter = ControlInput(nameof(Enter), OnEnter);
            Retrieved = ControlOutput(nameof(Retrieved));

            Pool = ValueInput<ObjectPool>(nameof(Pool), null).NullMeansSelf();

            Result = ValueOutput<GameObject>(nameof(Result));

            Succession(Enter, Retrieved);
            Assignment(Enter, Result);
        }

        private ControlOutput OnEnter(Flow flow)
        {
            var pool = flow.GetValue<ObjectPool>(Pool);

            if (pool != null)
            {
                GameObject obj = pool.RetrieveObjectFromPool();
                flow.SetValue(Result, obj);
            }

            return Retrieved;
        }
    }
}