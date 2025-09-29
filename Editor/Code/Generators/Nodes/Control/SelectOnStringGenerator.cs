using System;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(SelectOnString))]
    public sealed class SelectOnStringGenerator : NodeGenerator<SelectOnString>
    {
        public SelectOnStringGenerator(SelectOnString unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            var selector = GenerateValue(Unit.selector, data);
            var defaultValue = GenerateValue(Unit.@default, data);
            var currentIndent = CodeBuilder.GetCurrentIndent();
            var indent = CodeBuilder.GetCurrentIndent(1);

            var cases = Unit.branches.Select(branch =>
            {
                var key = branch.Key.ToLowerInvariant() ?? "null";
                var value = GenerateValue(branch.Value, data);
                var keyCode = key.As().Code(false);
                return indent + MakeClickableForThisUnit(keyCode + " ") + MakeClickableForThisUnit($"=> ") + value;
            });

            var defaultCase = indent + MakeClickableForThisUnit("_ ".VariableHighlight()) + MakeClickableForThisUnit($"=> ") + defaultValue;

            var body = string.Join($"{MakeClickableForThisUnit(",")}\n", cases.Concat(new[] { defaultCase }));

            return selector + MakeClickableForThisUnit((Unit.ignoreCase ? ".ToLowerInvariant()" : "") +" switch".ControlHighlight()) + "\n" +
                   currentIndent + MakeClickableForThisUnit("{") + "\n" +
                   body + "\n" +
                   currentIndent + MakeClickableForThisUnit("}");
        }

    }
}