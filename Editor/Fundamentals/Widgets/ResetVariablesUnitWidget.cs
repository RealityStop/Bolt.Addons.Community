using System.Collections.Generic;

namespace Unity.VisualScripting.Community 
{
    [Widget(typeof(ResetSavedVariables))]
    public class ResetVariablesUnitWidget : UnitWidget<ResetSavedVariables>
    {
        public ResetVariablesUnitWidget(FlowCanvas canvas, ResetSavedVariables unit) : base(canvas, unit)
        {
            nameInspectorConstructor = (metadata) => new VisualScripting.VariableNameInspector(metadata, GetNameSuggestions);
        }
    
        private readonly Dictionary<IUnitPort, VisualScripting.VariableNameInspector> inspectorCache = new Dictionary<IUnitPort, VisualScripting.VariableNameInspector>();
        private readonly System.Func<Metadata, VisualScripting.VariableNameInspector> nameInspectorConstructor;
    
        protected override NodeColorMix baseColor => NodeColorMix.TealReadable;
    
        public override Inspector GetPortInspector(IUnitPort port, Metadata metadata)
        {
            if (port is ValueInput valueInput && unit.arguments.Contains(valueInput))
            {
                if (!inspectorCache.TryGetValue(port, out var inspector))
                {
                    inspector = new VisualScripting.VariableNameInspector(metadata, GetNameSuggestions);
                    inspectorCache[port] = inspector;
                }
                else
                {
                    InspectorProvider.instance.Renew(ref inspector, metadata, nameInspectorConstructor);
                }
    
                return inspector;
            }
    
            return base.GetPortInspector(port, metadata);
        }
    
        private IEnumerable<string> GetNameSuggestions()
        {
            return EditorVariablesUtility.GetVariableNameSuggestions(VariableKind.Saved, reference);
        }
    } 
}
