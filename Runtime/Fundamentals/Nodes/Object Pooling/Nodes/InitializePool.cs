using UnityEngine;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [UnitCategory("Community\\ObjectPooling")]
    [UnitTitle("Initialize Pool")]
    [UnitSurtitle("Object Pool")]
    [RenamedFrom("InitializePoolNode")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.InitializePoolNode")]
    public class InitializePoolNode : Unit
    {
        [Inspectable]
        [InspectorLabel("Custom Parent", "If enabled, lets you specify a GameObject to act as the parent for the pool. If disabled, a new parent GameObject will be created automatically.")]
        [InspectorExpandTooltip]
        [InspectorWide(false)]
        [InspectorToggleLeft]
        public bool CustomParent = true;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput Enter;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput Initialized;

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput Prefab;

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput InitialPoolSize;

        [DoNotSerialize]
        [PortLabel("Pool")]
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
            InitialPoolSize = ValueInput(nameof(InitialPoolSize), 10);


            ObjectPool = ValueOutput<ObjectPool>(nameof(ObjectPool));

            Succession(Enter, Initialized);
            Assignment(Enter, ObjectPool);
        }

        private ControlOutput OnEnter(Flow flow)
        {
            var prefab = flow.GetValue<GameObject>(Prefab);
            var initialPoolSize = flow.GetValue<int>(InitialPoolSize);

            flow.SetValue(ObjectPool, Community.ObjectPool.CreatePool(initialPoolSize, prefab, CustomParent ? flow.GetValue<GameObject>(parent) : null));

            return Initialized;
        }
    }

}
