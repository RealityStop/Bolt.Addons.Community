using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.ReturnAllObjectsToPoolNode")]
    [RenamedFrom("ReturnAllObjectsToPoolNode")]
    [UnitCategory("Community\\ObjectPooling")]
    [UnitTitle("Return All")]
    [UnitSurtitle("Object Pool")]
    public class ReturnAllObjectsToPoolNode : Unit
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

        protected override void Definition()
        {
            Enter = ControlInput(nameof(Enter), OnEnter);
            Exit = ControlOutput(nameof(Exit));

            Pool = ValueInput<ObjectPool>(nameof(Pool), null).NullMeansSelf();

            Succession(Enter, Exit);
        }

        private ControlOutput OnEnter(Flow flow)
        {
            var pool = flow.GetValue<ObjectPool>(Pool);

            ObjectPool.ReturnAllObjects(pool);

            return Exit;
        }
    }


}