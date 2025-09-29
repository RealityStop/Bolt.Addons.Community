using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(YieldNode))]
    public class YieldNodeGenerator : NodeGenerator<YieldNode>
    {
        public YieldNodeGenerator(Unit unit) : base(unit) { }
        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            if (input == Unit.enter)
            {
                var output = string.Empty;

                switch (Unit.type)
                {
                    case YieldNode.EnumeratorType.YieldInstruction:
                        output += CodeBuilder.Indent(indent);
                        output += MakeClickableForThisUnit("yield ".ControlHighlight() + "return".ControlHighlight());
                        output += MakeClickableForThisUnit(" ");
                        output += GenerateValue(Unit.instruction, data);
                        output += MakeClickableForThisUnit(";") + "\n";
                        break;

                    case YieldNode.EnumeratorType.CustomYieldInstruction:
                        output += CodeBuilder.Indent(indent);
                        output += MakeClickableForThisUnit("yield ".ControlHighlight() + "return".ControlHighlight());
                        output += MakeClickableForThisUnit(" ");
                        output += GenerateValue(Unit.instruction, data);
                        output += MakeClickableForThisUnit(";") + "\n";
                        break;

                    case YieldNode.EnumeratorType.Enumerator:
                        var enumeratorVar = data.AddLocalNameInScope("enumerator", typeof(IEnumerator));
                        output += "\n";
                        output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit($"{"var".ConstructHighlight()} {enumeratorVar.VariableHighlight()} = ");
                        data.SetExpectedType(typeof(IEnumerator));
                        output += GenerateValue(Unit.enumerator, data) + MakeClickableForThisUnit(";") + "\n";
                        data.RemoveExpectedType();
                        output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit($"{"while".ControlHighlight()} ({enumeratorVar.VariableHighlight()}.MoveNext())") + "\n";
                        output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("{") + "\n";
                        output += CodeBuilder.Indent(indent + 1) + MakeClickableForThisUnit("yield return ".ControlHighlight() + enumeratorVar.VariableHighlight() + $".{"Current".VariableHighlight()};") + "\n";
                        output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("}") + "\n";
                        break;

                    case YieldNode.EnumeratorType.Coroutine:
                        output += CodeBuilder.Indent(indent);
                        output += MakeClickableForThisUnit("yield ".ControlHighlight() + "return".ControlHighlight());
                        output += MakeClickableForThisUnit(" ");
                        output += GenerateValue(Unit.coroutine, data);
                        output += MakeClickableForThisUnit(";") + "\n";
                        break;
                }
                output += GetNextUnit(Unit.exit, data, indent);
                return output;
            }

            return base.GenerateControl(input, data, indent);
        }
    }
}