using UnityEngine;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.OnKeysPressed")]
    [UnitCategory("Events\\Community\\Input")]
    [UnitTitle("On Multi-Key Press")]
    [RenamedFrom("Bolt.Addons.Community.Logic.Units.OnDualKeyInput")]
    [RenamedFrom("Unity.VisualScripting.Community.OnKeysPressed")]
    [TypeIcon(typeof(OnKeyboardInput))]
    public class OnMultiKeyPress : MachineEventUnit<EmptyEventArgs>
    {
        public new class Data : EventUnit<EmptyEventArgs>.Data
        {
            public KeyCode key1;
            public KeyCode key2;
            public PressState state;

            public bool key1Down;
            public bool key2Down;
            public bool key1Up;
            public bool key2Up;

            public float key1Time;
            public float key2Time;
        }

        public override IGraphElementData CreateData()
        {
            return new Data();
        }

        protected override string hookName => EventHooks.Update;

        [DoNotSerialize]
        [PortLabel("Key 1")]
        public ValueInput Key1 { get; private set; }

        [DoNotSerialize]
        [PortLabel("Key 2")]
        public ValueInput Key2 { get; private set; }

        [DoNotSerialize]
        [PortLabel("Action")]
        public ValueInput Action { get; private set; }

        /// <summary>
        /// Maximum delay between key presses for them to count as the same multi-key action.
        /// </summary>
        [InspectorExpandTooltip]
        [InspectorLabel("Delay", "Maximum delay between key presses for them to count as the same multi-key action.")]
        [Inspectable]
        [InspectorWide]
        public float Delay { get; set; } = 0.2f;

        protected override void Definition()
        {
            base.Definition();

            Key1 = ValueInput(nameof(Key1), KeyCode.None);
            Key2 = ValueInput(nameof(Key2), KeyCode.None);
            Action = ValueInput(nameof(Action), PressState.Down);
        }

        protected override bool ShouldTrigger(Flow flow, EmptyEventArgs args)
        {
            var data = flow.stack.GetElementData<Data>(this);

            if (!data.isListening) return false;

            data.key1 = flow.GetValue<KeyCode>(Key1);
            data.key2 = flow.GetValue<KeyCode>(Key2);
            data.state = flow.GetValue<PressState>(Action);

            if (Input.GetKeyDown(data.key1))
            {
                data.key1Down = true;
                data.key1Time = Time.time;
            }
            if (Input.GetKeyDown(data.key2))
            {
                data.key2Down = true;
                data.key2Time = Time.time;
            }

            if (Input.GetKeyUp(data.key1))
            {
                data.key1Up = true;
                data.key1Time = Time.time;
            }
            if (Input.GetKeyUp(data.key2))
            {
                data.key2Up = true;
                data.key2Time = Time.time;
            }

            if (data.state == PressState.Down)
            {
                if (data.key1Down && data.key2Down && Mathf.Abs(data.key1Time - data.key2Time) <= Delay)
                {
                    data.key1Down = false;
                    data.key2Down = false;
                    return true;
                }
            }

            if (data.state == PressState.Hold)
            {
                return Input.GetKey(data.key1) && Input.GetKey(data.key2);
            }

            if (data.state == PressState.Up)
            {
                if (data.key1Up && data.key2Up && Mathf.Abs(data.key1Time - data.key2Time) <= Delay)
                {
                    data.key1Up = false;
                    data.key2Up = false;
                    return true;
                }
            }

            return false;
        }
    }
}