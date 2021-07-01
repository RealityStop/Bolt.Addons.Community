using Unity.VisualScripting;

#if VISUAL_SCRIPTING_1_7
using SMachine = Unity.VisualScripting.ScriptMachine;
#else
using SMachine = Unity.VisualScripting.FlowMachine;
#endif

namespace Bolt.Addons.Community.Fundamentals
{
    [UnitCategory("Community/Graphs")]
    [TypeIcon(typeof(SMachine))]
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
            target = ValueInput<SMachine>("target", (SMachine)null);
            target.NullMeansSelf();
            name = ValueInput<string>("name", defaultName);
        }
    }
}