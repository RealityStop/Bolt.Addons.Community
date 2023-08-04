using UnityEngine;
using Unity.VisualScripting;


    [UnitCategory("Community\\ObjectPooling")]
    [UnitTitle("Return Object")]
    public class ReturnObjectNode : Unit
    {
        [DoNotSerialize]
        public ControlInput Enter;

        [DoNotSerialize]
        public ControlOutput Exit;

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput Pool;

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput ObjectToReturn;

        protected override void Definition()
        {
            Enter = ControlInput(nameof(Enter), OnEnter);
            Exit = ControlOutput(nameof(Exit));

            Pool = ValueInput<CustomObjectPool>(nameof(Pool));
            ObjectToReturn = ValueInput<GameObject>(nameof(ObjectToReturn));

            Succession(Enter, Exit);
        }

        private ControlOutput OnEnter(Flow flow)
        {
            var pool = flow.GetValue<CustomObjectPool>(Pool);
            var obj = flow.GetValue<GameObject>(ObjectToReturn);

            if (pool != null && obj != null)
            {
                pool.ReturnObjectToPool(obj);
            }

            return Exit;
        }
    }

