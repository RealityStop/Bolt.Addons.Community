using UnityEngine;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Fundamentals
{
    [UnitCategory("Events\\Community\\Input")]
    [UnitTitle("OnKeysPressed")]
    [RenamedFrom("Bolt.Addons.Community.Logic.Units.OnDualKeyInput")]
    [TypeIcon(typeof(OnKeyboardInput))]
    public class OnKeysPressed : MachineEventUnit<EmptyEventArgs>
    {
        public new class Data : EventUnit<EmptyEventArgs>.Data
        {
            public KeyCode key1;
            public KeyCode key2;
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

        protected override void Definition()
        {
            base.Definition();

            Key1 = ValueInput(nameof(Key1), KeyCode.None);
            Key2 = ValueInput(nameof(Key2), KeyCode.None);
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

            data.key1 = flow.GetValue<KeyCode>(Key1);
            data.key2 = flow.GetValue<KeyCode>(Key2);

            if (Input.GetKey(data.key1) && Input.GetKey(data.key2))
            {
                return true;
            }

            return false;
        }
    }
}
