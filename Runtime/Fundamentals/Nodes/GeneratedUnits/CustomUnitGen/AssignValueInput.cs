using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

[UnitTitle("AssignValueInput")]//Unit title
[UnitCategory("Community/CodeGenerators/Unit")]
[TypeIcon(typeof(Flow))]//Unit icon
public class AssignValueInput : GeneratedUnit
{
    [DoNotSerialize]
    [PortLabelHidden]
    public ValueInput VariableName;

    [DoNotSerialize]
    [PortLabel("NullMeansSelf")]
    public ValueInput NullMeansSelf;

    [UnitHeaderInspectable("Type")]
    public Type VariableType;

    protected override void Definition()
    {
        base.Definition();

        VariableName = ValueInput<string>(nameof(VariableName), default);
        if (VariableType == typeof(GameObject))
        {
            NullMeansSelf = ValueInput<bool>(nameof(NullMeansSelf), default);
        }

        Requirement(VariableName, Enter);
    }

    public override string GeneratorLogic(ControlGenerationData data, int indent, NodeGenerator generator)
    {
        bool nullmeansSelf = false;
        if (VariableType == typeof(GameObject)) 
        {
            nullmeansSelf = bool.Parse(generator.GenerateValue(NullMeansSelf));
        }

        string assign = $"{generator.GenerateValue(VariableName)} = ValueInput<{VariableType.CSharpFullName()}>(nameof({generator.GenerateValue(VariableName)}), default)" + (nullmeansSelf ? "" : ";");
        string shouldmeanSelf = nullmeansSelf ? $".NullMeansSelf();" : "";
        return assign + shouldmeanSelf + "\n";
    }
}