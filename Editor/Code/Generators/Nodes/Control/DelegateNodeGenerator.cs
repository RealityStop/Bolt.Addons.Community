using System;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(DelegateNode))]
    public sealed class DelegateNodeGenerator : NodeGenerator<DelegateNode>
    {
        public int indent = 0;
        private bool shouldIndent;
        public DelegateNodeGenerator(DelegateNode unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            this.indent = indent + 1;
            var output = string.Empty;
            shouldIndent = true;
            output += MakeSelectableForThisUnit($"({GetParameters()}) =>") + $"\n{MakeSelectableForThisUnit("{")}\n{CodeBuilder.Indent(indent)}{(Unit.invoke.hasAnyConnection ? (Unit.invoke.connection.destination.unit as Unit).GenerateControl(Unit.invoke.connection.destination, new ControlGenerationData(), indent + 1) : string.Empty)}{(Unit is FuncNode func ? GenerateValue(func.@return, data) : string.Empty)}\n{MakeSelectableForThisUnit("}")}";
            return output;
        }

        private string GetParameters()
        {
            return string.Join(", ", Unit.parameters.Select(param => param.key));
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeSelectableForThisUnit(output.key);
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (Unit is FuncNode funcNode)
            {
                if (input == funcNode.@return && funcNode.@return.hasValidConnection)
                {
                    return (shouldIndent ? CodeBuilder.Indent(indent) : string.Empty) + MakeSelectableForThisUnit("return".ControlHighlight() + " ") + (input.connection.source.unit as Unit).GenerateValue(input.connection.source, data) + MakeSelectableForThisUnit(";");
                }
            }
            return base.GenerateValue(input, data);
        }
    }
}