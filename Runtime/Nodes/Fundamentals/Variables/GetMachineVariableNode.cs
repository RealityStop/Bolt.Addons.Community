#if VISUAL_SCRIPTING_1_7
using SMachine = Unity.VisualScripting.ScriptMachine;
#else
using SMachine = Unity.VisualScripting.FlowMachine;
#endif

namespace Unity.VisualScripting.Community
{
    [UnitTitle("Get Machine Variable")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.GetMachineVariableUnit")]
    public sealed class GetMachineVariableNode : MachineVariableNode
    {
        [DoNotSerialize]
        public ValueOutput value;

        protected override void Definition()
        {
            base.Definition();
            value = ValueOutput<object>(nameof(value), (flow) => 
            {
                return Variables.Graph(flow.GetValue<SMachine>(target).GetReference()).Get(flow.GetValue<string>(name));
            });
        }
    }
}