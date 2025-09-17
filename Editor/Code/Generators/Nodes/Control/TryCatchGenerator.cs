using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

[NodeGenerator(typeof(TryCatch))]
public class TryCatchGenerator : LocalVariableGenerator
{
    private TryCatch Unit => unit as TryCatch;
    public TryCatchGenerator(Unit unit) : base(unit)
    {
    }

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        var output = "";
        if (Unit.@try.hasValidConnection)
        {
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("try".ControlHighlight()) + "\n";
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("{") + "\n";
            data.NewScope();
            output += GetNextUnit(Unit.@try, data, indent + 1);
            data.ExitScope();
            output += "\n" + CodeBuilder.Indent(indent) + MakeClickableForThisUnit("}") + "\n";

            if (!Unit.@catch.hasValidConnection && !Unit.@finally.hasValidConnection)
            {
                return output + "/* Catch or Finally requires connection */".WarningHighlight();
            }

            if (Unit.@catch.hasValidConnection)
            {
                data.NewScope();
                variableName = data.AddLocalNameInScope("ex", Unit.exceptionType);
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("catch".ControlHighlight() + " (" + Unit.exceptionType.As().CSharpName(false, true) + $" {variableName}".VariableHighlight() + ")") + "\n";
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("{") + "\n";
                output += GetNextUnit(Unit.@catch, data, indent + 1);
                data.ExitScope();
                output += "\n" + CodeBuilder.Indent(indent) + MakeClickableForThisUnit("}") + "\n";
            }

            if (Unit.@finally.hasValidConnection)
            {
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("finally".ControlHighlight()) + "\n";
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("{") + "\n";
                data.NewScope();
                output += GetNextUnit(Unit.@finally, data, indent + 1);
                data.ExitScope();
                output += "\n" + CodeBuilder.Indent(indent) + MakeClickableForThisUnit("}") + "\n";
            }
        }
        return output;
    }

    public override string GenerateValue(ValueOutput output, ControlGenerationData data)
    {
        if (data.ContainsNameInAnyScope(data.GetVariableName(variableName)))
            return MakeClickableForThisUnit(variableName.VariableHighlight());
        else
            return MakeClickableForThisUnit($"/* {variableName} is only accessible from a catch scope */".WarningHighlight());
    }
}
