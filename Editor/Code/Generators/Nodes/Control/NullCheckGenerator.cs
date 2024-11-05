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
            var _input = GenerateValue(Unit.input, data);
            if (Unit.ifNotNull.hasValidConnection)
            {
                output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit("if".ConstructHighlight() + "(") + $"{_input}" + MakeSelectableForThisUnit($" != {"null".ConstructHighlight()})") + "\n";
                output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit("{") + "\n";
                data.NewScope();
                output += GetNextUnit(Unit.ifNotNull, data, indent + 1);
                data.ExitScope();
                output += "\n" + CodeBuilder.Indent(indent) + MakeSelectableForThisUnit("}") + "\n";
                if (Unit.ifNull.hasValidConnection)
                {
                    output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit($"else".ConstructHighlight()) + "\n";
                    output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit("{") + "\n";
                    data.NewScope();
                    output += GetNextUnit(Unit.ifNull, data, indent + 1);
                    data.ExitScope();
                    output += "\n" + CodeBuilder.Indent(indent) + MakeSelectableForThisUnit("}") + "\n";
                }
            }
            else if (Unit.ifNull.hasValidConnection)
            {
                output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit("if".ConstructHighlight() + "(") + $"{_input}" + MakeSelectableForThisUnit($" == {"null".ConstructHighlight()})") + "\n";
                output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit("{") + "\n";
                data.NewScope();
                output += GetNextUnit(Unit.ifNull, data, indent + 1);
                data.ExitScope();
                output += "\n" + CodeBuilder.Indent(indent) + MakeSelectableForThisUnit("}") + "\n";
            }

            return output;
        }
    }
}