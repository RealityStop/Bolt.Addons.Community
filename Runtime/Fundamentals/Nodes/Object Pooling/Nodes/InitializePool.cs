using UnityEngine;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{

    [UnitCategory("Community\\ObjectPooling")]
    [UnitTitle("Initialize Pool")]
    public class InitializePoolNode : Unit
    {
        [UnitHeaderInspectable("CustomParent")]
        public bool CustomParent = true;
        [DoNotSerialize]
        public ControlInput Enter;

        [DoNotSerialize]
        public ControlOutput Initialized;

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput Prefab;

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput InitialPoolSize;

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput ObjectPool;

        [DoNotSerialize]
        [NullMeansSelf]
        [PortLabelHidden]
        public ValueInput parent;

        protected override void Definition()
        {
            Enter = ControlInput(nameof(Enter), OnEnter);
            Initialized = ControlOutput(nameof(Initialized));

            if (CustomParent)
            {
                parent = ValueInput<GameObject>(nameof(parent), default).NullMeansSelf();
            }
            Prefab = ValueInput<GameObject>(nameof(Prefab), null);
            InitialPoolSize = ValueInput<int>(nameof(InitialPoolSize), 10);


            ObjectPool = ValueOutput<CustomObjectPool>(nameof(ObjectPool));

            Succession(Enter, Initialized);
            Assignment(Enter, ObjectPool);
        }

        private ControlOutput OnEnter(Flow flow)
        {
            var prefab = flow.GetValue<GameObject>(Prefab);
            var initialPoolSize = flow.GetValue<int>(InitialPoolSize);

            GameObject poolParentGO = null;
            if (CustomParent)
            {
                poolParentGO = flow.GetValue<GameObject>(parent);
            }
            else
            {
                poolParentGO = new GameObject("ObjectPoolParent");
            }
            var customObjectPool = poolParentGO.AddComponent<CustomObjectPool>();
            flow.SetValue(ObjectPool, poolParentGO.GetComponent<CustomObjectPool>());
            customObjectPool.Initialize(prefab, initialPoolSize);

            // Set all pooled objects as children of the pool's parent
            var children = poolParentGO.GetComponentsInChildren<Transform>();
            foreach (var child in children)
            {
                if (child != poolParentGO.transform)
                {
                    child.SetParent(poolParentGO.transform);
                }
            }

            return Initialized;
        }
    }

}
