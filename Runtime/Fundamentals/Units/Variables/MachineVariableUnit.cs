using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Fundamentals
{
    [UnitCategory("Community/Graphs")]
    [TypeIcon(typeof(ScriptMachine))]
    public abstract class MachineVariableUnit : Unit
    {
        [Serialize]
        [Inspectable]
        [UnitHeaderInspectable]
        public ScriptGraphAsset asset;
        [Serialize]
        public string defaultName = string.Empty;
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput name;
        [DoNotSerialize]
        [NullMeansSelf]
        [PortLabelHidden]
        public ValueInput target;

        protected override void Definition()
        {
            target = ValueInput<ScriptMachine>("target", (ScriptMachine)null);
            target.NullMeansSelf();
            name = ValueInput<string>("name", defaultName);
        }
    }
}