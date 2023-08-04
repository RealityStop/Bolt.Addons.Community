using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

    [UnitCategory("Community\\ObjectPooling")]
    [UnitTitle("Return All")]
    public class ReturnAllObjectsToPoolNode : Unit
    {
        [DoNotSerialize]
        public ControlInput Enter;

        [DoNotSerialize]
        public ControlOutput Exit;

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput Pool;

        protected override void Definition()
        {
            Enter = ControlInput(nameof(Enter), OnEnter);
            Exit = ControlOutput(nameof(Exit));

            Pool = ValueInput<CustomObjectPool>(nameof(Pool));

            Succession(Enter, Exit);
        }

        private ControlOutput OnEnter(Flow flow)
        {
            var pool = flow.GetValue<CustomObjectPool>(Pool);

            if (pool != null)
            {
                List<GameObject> activeObjectsCopy = new List<GameObject>(pool.GetActiveObjects());

                foreach (var obj in activeObjectsCopy)
                {
                    pool.ReturnObjectToPool(obj);
                }
            }

            return Exit;
        }
    }

