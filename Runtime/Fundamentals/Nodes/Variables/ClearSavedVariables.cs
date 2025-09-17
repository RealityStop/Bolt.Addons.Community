using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("ClearSavedVars")]
    [RenamedFrom("Unity.VisualScripting.Community.ClearSavedVars")]
    [UnitCategory("Community/Variables")]
    [UnitTitle("Clear")]
    [UnitSurtitle("Saved Variables")]
    [TypeIcon(typeof(FlowGraph))]
    public class ClearSavedVariables : Unit
    {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput In;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput Out;

        [DoNotSerialize]
        public ValueInput Key;

        protected override void Definition()
        {
            In = ControlInput(nameof(In), Enter);
            Out = ControlOutput(nameof(Out));
            Succession(In, Out);
        }

        public ControlOutput Enter(Flow flow)
        {
            CSharpUtility.ClearSavedVariables();
            return Out;
        }
    }

}