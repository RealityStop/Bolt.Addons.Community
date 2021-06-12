using Unity.VisualScripting;

namespace Bolt.Addons.Community.Fundamentals
{
    public sealed class SetMachineVariableUnit : MachineVariableUnit
    {
        [DoNotSerialize]
        public ControlInput enter;
        [DoNotSerialize]
        public ControlOutput exit;
        [DoNotSerialize]
        public ValueInput value;

        protected override void Definition()
        {
            base.Definition();
            enter = ControlInput("enter", (flow) =>
            {
                Variables.Graph(flow.GetValue<ScriptMachine>(target).GetReference()).Set(flow.GetValue<string>(name), flow.GetValue(value));
                return exit;
            });
            exit = ControlOutput("exit");

            value = ValueInput<object>("value");
        }
    }
}