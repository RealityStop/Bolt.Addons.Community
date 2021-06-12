using System;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Fundamentals
{
    public sealed class GetMachineVariableUnit : MachineVariableUnit
    {
        [DoNotSerialize]
        public ValueOutput value;

        protected override void Definition()
        {
            base.Definition();
            value = ValueOutput<object>(nameof(value), (flow) => 
            {
                return Variables.Graph(flow.GetValue<ScriptMachine>(target).GetReference()).Get(flow.GetValue<string>(name));
            });
        }
    }
}