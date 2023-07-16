using UnityEngine;
using Unity.VisualScripting;


    [UnitCategory("Community\\ObjectPooling")]
    [UnitTitle("Retrieve Object")]
    public class RetrieveObjectNode : Unit
    {
        [DoNotSerialize]
        public ControlInput Enter;

        [DoNotSerialize]
        public ControlOutput Retrieved;

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput Pool;

        [DoNotSerialize]
        public ValueOutput Result;

        protected override void Definition()
        {
            Enter = ControlInput(nameof(Enter), OnEnter);
            Retrieved = ControlOutput(nameof(Retrieved));

            Pool = ValueInput<CustomObjectPool>(nameof(Pool));

            Result = ValueOutput<GameObject>(nameof(Result));

            Succession(Enter, Retrieved);
            Assignment(Enter, Result);
        }

        private ControlOutput OnEnter(Flow flow)
        {
            var pool = flow.GetValue<CustomObjectPool>(Pool);

            if (pool != null)
            {
                GameObject obj = pool.RetrieveObjectFromPool();
                flow.SetValue(Result, obj);
            }

            return Retrieved;
        }
    }

