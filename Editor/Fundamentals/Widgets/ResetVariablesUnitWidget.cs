using System.Collections.Generic;

namespace Unity.VisualScripting.Community 
{
    [Widget(typeof(ResetSavedVariables))]
    public class ResetVariablesUnitWidget : UnitWidget<ResetSavedVariables>
    {
        public ResetVariablesUnitWidget(FlowCanvas canvas, ResetSavedVariables unit) : base(canvas, unit)
        {
            nameInspectorConstructor = (metadata) => new VariableNameInspector(metadata, GetNameSuggestions);
        }
    
        private readonly Dictionary<IUnitPort, VariableNameInspector> inspectorCache = new();
        private readonly System.Func<Metadata, VariableNameInspector> nameInspectorConstructor;
    
        protected override NodeColorMix baseColor => NodeColorMix.TealReadable;
    
        public override Inspector GetPortInspector(IUnitPort port, Metadata metadata)
        {
            if (port is ValueInput valueInput && unit.arguments.Contains(valueInput))
            {
                if (!inspectorCache.TryGetValue(port, out var inspector))
                {
                    inspector = new VariableNameInspector(metadata, GetNameSuggestions);
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
