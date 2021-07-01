using Unity.VisualScripting;

#if VISUAL_SCRIPTING_1_7
using SMachine = Unity.VisualScripting.ScriptMachine;
#else
using SMachine = Unity.VisualScripting.FlowMachine;
#endif

namespace Bolt.Addons.Community.Fundamentals
{
    [UnitTitle("Set Machine Variable")]
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
                Variables.Graph(flow.GetValue<SMachine>(target).GetReference()).Set(flow.GetValue<string>(name), flow.GetValue(value));
                return exit;
            });
            exit = ControlOutput("exit");

            value = ValueInput<object>("value");
        }
    }
}