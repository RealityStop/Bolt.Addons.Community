using System;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(SelectOnEnum))]
    public sealed class SelectOnEnumGenerator : NodeGenerator<SelectOnEnum>
    {
        public SelectOnEnumGenerator(SelectOnEnum unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            var selectorCode = GenerateValue(Unit.selector, data);
            var currentIndent = CodeBuilder.GetCurrentIndent();
            var indent = CodeBuilder.GetCurrentIndent(1);

            var cases = Unit.branches.Select(branch =>
            {
                var keyCode = $"{branch.Key.As().Code(false)}";

                var valueCode = GenerateValue(branch.Value, data);

                return indent
                     + MakeClickableForThisUnit(keyCode + " ")
                     + MakeClickableForThisUnit("=> ")
                     + valueCode;
            });

            var defaultCase = indent
                            + MakeClickableForThisUnit("_ ".VariableHighlight())
                            + MakeClickableForThisUnit("=> ")
                            + MakeClickableForThisUnit("default".ConstructHighlight());

            var body = string.Join(
                MakeClickableForThisUnit(",") + "\n",
                cases.Concat(new[] { defaultCase })
            );

            return selectorCode
                 + MakeClickableForThisUnit(" switch".ControlHighlight()) + "\n"
                 + currentIndent + MakeClickableForThisUnit("{") + "\n"
                 + body + "\n"
                 + currentIndent + MakeClickableForThisUnit("}");
        }
    }
}