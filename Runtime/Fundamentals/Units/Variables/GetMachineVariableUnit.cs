using System;
using Unity.VisualScripting;

#if VISUAL_SCRIPTING_1_7
using SMachine = Unity.VisualScripting.ScriptMachine;
#else
using SMachine = Unity.VisualScripting.FlowMachine;
#endif

namespace Bolt.Addons.Community.Fundamentals
{
    [UnitTitle("Get Machine Variable")]
    public sealed class GetMachineVariableUnit : MachineVariableUnit
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