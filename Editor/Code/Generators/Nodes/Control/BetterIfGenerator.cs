using Unity;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

[NodeGenerator(typeof(Unity.VisualScripting.Community.BetterIf))]
public sealed class BetterIfGenerator : NodeGenerator<Unity.VisualScripting.Community.BetterIf>
{
    public BetterIfGenerator(Unity.VisualScripting.Community.BetterIf unit) : base(unit)
    {
    }

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        var output = string.Empty;

        if (input == Unit.In)
        {

            output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + "if".ConstructHighlight() + " (" + GenerateValue(Unit.Condition, data) + ")");
            output += "\n";
            output += CodeUtility.MakeSelectable(Unit, CodeBuilder.OpenBody(indent));
            output += "\n";
            output += Unit.True.hasAnyConnection ? (Unit.True.connection.destination.unit as Unit).GenerateControl(Unit.True.connection.destination, data, indent + 1) : string.Empty;
            output += "\n";
            output += CodeUtility.MakeSelectable(Unit, CodeBuilder.CloseBody(indent));
            if (Unit.False.hasAnyConnection)
            {
                output += "\n";
                output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + "else".ConstructHighlight()) + (!Unit.True.hasValidConnection ? CodeBuilder.MakeRecommendation("You should use the negate node and connect the true input instead") : string.Empty);
                output += "\n";
                output += CodeUtility.MakeSelectable(Unit, CodeBuilder.OpenBody(indent));
                output += "\n";
                output += Unit.False.hasAnyConnection ? (Unit.False.connection.destination.unit as Unit).GenerateControl(Unit.False.connection.destination, data, indent + 1) : string.Empty;
                output += "\n";
                output += CodeUtility.MakeSelectable(Unit, CodeBuilder.CloseBody(indent));
            }

            if (Unit.Finished.hasAnyConnection)
            {
                output += "\n";
                output += (Unit.Finished.connection.destination.unit as Unit).GenerateControl(Unit.Finished.connection.destination, data, indent);
            }
        }

        return output;
    }

    public override string GenerateValue(ValueInput input, ControlGenerationData data)
    {
        if (input == Unit.Condition)
        {
            if (Unit.Condition.hasAnyConnection)
            {
                return CodeUtility.MakeSelectable(input.connection.source.unit as Unit , new ValueCode((input.connection.source.unit as Unit).GenerateValue(input.connection.source, data), input.type, ShouldCast(input)));
            }
        }
        return base.GenerateValue(input, data);
    }
}
