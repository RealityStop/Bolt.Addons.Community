using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(NullCheck))]
    public class NullCheckGenerator : NodeGenerator<NullCheck>
    {
        public NullCheckGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;
            var _input = Unit.input.hasValidConnection ? CodeUtility.MakeSelectable(Unit.input.connection.source.unit as Unit, GenerateValue(Unit.input)) : GenerateValue(Unit.input);
            if (Unit.ifNotNull.hasValidConnection)
            {
                output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + $"{"if".ControlHighlight()}({_input} != {"null".ConstructHighlight()})") + "\n";
                output += CodeUtility.MakeSelectable(Unit, CodeBuilder.OpenBody(indent)) + "\n";
                data.NewScope();
                output += GetNextUnit(Unit.ifNotNull, data, indent + 1);
                data.ExitScope();
                output += CodeUtility.MakeSelectable(Unit, CodeBuilder.CloseBody(indent)) + "\n";
                if (Unit.ifNull.hasValidConnection)
                {
                    output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + $"else".ControlHighlight()) + "\n";
                    output += CodeUtility.MakeSelectable(Unit, CodeBuilder.OpenBody(indent)) + "\n";
                    data.NewScope();
                    output += GetNextUnit(Unit.ifNotNull, data, indent + 1);
                    data.ExitScope();
                    output += CodeUtility.MakeSelectable(Unit, CodeBuilder.CloseBody(indent)) + "\n";
                }
            }
            else if (Unit.ifNull.hasValidConnection)
            {
                output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + $"if({_input} == {"null".ConstructHighlight()})") + "\n";
                output += CodeUtility.MakeSelectable(Unit, CodeBuilder.OpenBody(indent)) + "\n";
                data.NewScope();
                output += GetNextUnit(Unit.ifNull, data, indent + 1);
                data.ExitScope();
                output += CodeUtility.MakeSelectable(Unit, CodeBuilder.CloseBody(indent)) + "\n";
            }

            return output;
        }
    }
}