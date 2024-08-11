using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
[RenamedFrom("ClearSavedVars")]    
    [UnitCategory("Community/Variables")]
    [UnitTitle("ClearSavedVars")]
    [TypeIcon(typeof(FlowGraph))]
    public class ClearSavedVars : Unit
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
            In = ControlInput(nameof(In), _Enter_);
            Out = ControlOutput(nameof(Out));
            Succession(In, Out);
        }
    
        public ControlOutput _Enter_(Flow flow)
        {
            SavedVariables.saved.Clear();
            SavedVariables.SaveDeclarations(SavedVariables.saved);
            return Out;
        }
    }
    
}