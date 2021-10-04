using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// Called whenever a specified number of seconds have elapsed.
    /// </summary>
    [UnitCategory("Events/Community/Time")]
    [RenamedFrom("Bolt.Addons.Community.Logic.Units.OnEveryXSeconds")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.OnEveryXSeconds")]
    public sealed class OnEveryXSeconds : MachineEventUnit<EmptyEventArgs>
    {
        public new class Data : EventUnit<EmptyEventArgs>.Data
        {
            public float time;
        }

        public override IGraphElementData CreateData()
        {
            return new Data();
        }

        protected override string hookName => EventHooks.Update;

        /// <summary>
        /// The number of seconds to await.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("Delay")]
        public ValueInput seconds { get; private set; }

        /// <summary>
        /// Whether to ignore the time scale.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("Unscaled")]
        public ValueInput unscaledTime { get; private set; }

        protected override void Definition()
        {
            base.Definition();

            seconds = ValueInput(nameof(seconds), 0f);
            unscaledTime = ValueInput(nameof(unscaledTime), false);
        }

        public override void StartListening(GraphStack stack)
        {
            base.StartListening(stack);

            var data = stack.GetElementData<Data>(this);
        }

        protected override bool ShouldTrigger(Flow flow, EmptyEventArgs args)
        {
            var data = flow.stack.GetElementData<Data>(this);

            if (!data.isListening)
            {
                return false;
            }

            //Requery the time to run for, in case we're wired to a dynamic value.
            var increment = flow.GetValue<bool>(unscaledTime) ? Time.unscaledDeltaTime : Time.deltaTime;
            var threshold = flow.GetValue<float>(seconds);

            data.time += increment;

            if (data.time >= threshold)
            {
                data.time = 0;
                return true;
            }

            return false;
        }
    }
}