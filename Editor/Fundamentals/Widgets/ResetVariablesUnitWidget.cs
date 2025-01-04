using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

[Widget(typeof(ResetSavedVariables))]
public class ResetVariablesUnitWidget : UnitWidget<ResetSavedVariables>
{
    public ResetVariablesUnitWidget(FlowCanvas canvas, ResetSavedVariables unit) : base(canvas, unit)
    {
        nameInspectorConstructor = (metadata) => new VariableNameInspector(metadata, GetNameSuggestions);
    }

    private VariableNameInspector nameInspector;
    private System.Func<Metadata, VariableNameInspector> nameInspectorConstructor;
    protected override NodeColorMix baseColor => NodeColorMix.TealReadable;

    public override Inspector GetPortInspector(IUnitPort port, Metadata metadata)
    {
        if (port is ValueInput valueInput)
        {
            if (unit.Keys.Contains(valueInput))
            {
                InspectorProvider.instance.Renew(ref nameInspector, metadata, nameInspectorConstructor);
                return nameInspector;
            }
        }

        return base.GetPortInspector(port, metadata);
    }


    private IEnumerable<string> GetNameSuggestions()
    {
        return EditorVariablesUtility.GetVariableNameSuggestions(VariableKind.Saved, reference);
    }
}
