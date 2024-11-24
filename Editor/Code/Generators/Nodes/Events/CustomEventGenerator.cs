using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

[NodeGenerator(typeof(CustomEvent))]
public class CustomEventGenerator : NodeGenerator<CustomEvent>
{
    public CustomEventGenerator(Unit unit) : base(unit)
    {
    }

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        return GetNextUnit(Unit.trigger, data, indent);
    }

    public override string GenerateValue(ValueInput input, ControlGenerationData data)
    {
        if (input == Unit.target && !input.hasValidConnection)
            return MakeSelectableForThisUnit("gameObject".VariableHighlight());
        return base.GenerateValue(input, data);
    }
    public override string GenerateValue(ValueOutput output, ControlGenerationData data)
    {
        if (Unit.argumentPorts.Contains(output))
        {
            var callCode = CodeBuilder.CallCSharpUtilityExtensitionMethod(Unit, MakeSelectableForThisUnit("args".VariableHighlight()), MakeSelectableForThisUnit(nameof(CSharpUtility.GetArgument)), MakeSelectableForThisUnit(Unit.argumentPorts.IndexOf(output).ToString()), MakeSelectableForThisUnit(((object)(data.GetExpectedType() ?? typeof(object))).As().Code(false, false, true, "", false, true)));
            var code = new ValueCode(callCode, data.GetExpectedType(), data.GetExpectedType() != null && !data.IsCurrentExpectedTypeMet() && data.GetExpectedType() != typeof(object));
            data.CreateSymbol(Unit, typeof(object), code);
            return code;
        }
        return base.GenerateValue(output, data);
    }
}
