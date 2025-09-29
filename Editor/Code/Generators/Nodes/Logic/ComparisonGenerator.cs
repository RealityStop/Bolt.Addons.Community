using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Unity.VisualScripting.Comparison))]
    public class ComparisonGenerator : NodeGenerator<Comparison>
    {
        public ComparisonGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            string a = GenerateValue(Unit.a, data);
            string b = GenerateValue(Unit.b, data);
            bool numeric = Unit.numeric;

            string op(string symbol) => MakeClickableForThisUnit(symbol);
            string call(string method, string args) =>
                $"{op("OperatorUtility".TypeHighlight())}{op("." + method + "(")}{args}{op(")")}";

            string approx(string x, string y) =>
                $"{op("Mathf".TypeHighlight())}{op(".Approximately(")}{x}{op(", ")}{y}{op(")")}";

            string group(string expression) =>
                $"{op("(")}{expression}{op(")")}";

            string logicOr(string left, string right) =>
                $"{group(left + op(" || ") + right)}";

            if (numeric)
            {
                NameSpaces = "UnityEngine";
                if (output == Unit.aLessThanB)
                    return $"{a}{op(" < ")}{b}";

                if (output == Unit.aLessThanOrEqualToB)
                    return logicOr($"{a}{op(" < ")}{b}", approx(a, b));

                if (output == Unit.aEqualToB)
                    return approx(a, b);

                if (output == Unit.aNotEqualToB)
                    return $"{op("!")} {approx(a, b)}";

                if (output == Unit.aGreaterThanOrEqualToB)
                    return logicOr($"{a}{op(" > ")}{b}", approx(a, b));

                if (output == Unit.aGreatherThanB)
                    return $"{a}{op(" > ")}{b}";
            }
            else
            {
                NameSpaces = "Unity.VisualScripting";
                if (output == Unit.aLessThanB)
                    return call("LessThan", $"{a}{op(", ")}{b}");

                if (output == Unit.aLessThanOrEqualToB)
                    return call("LessThanOrEqual", $"{a}{op(", ")}{b}");

                if (output == Unit.aEqualToB)
                    return call("Equal", $"{a}{op(", ")}{b}");

                if (output == Unit.aNotEqualToB)
                    return call("NotEqual", $"{a}{op(", ")}{b}");

                if (output == Unit.aGreaterThanOrEqualToB)
                    return call("GreaterThanOrEqual", $"{a}{op(", ")}{b}");

                if (output == Unit.aGreatherThanB)
                    return call("GreaterThan", $"{a}{op(", ")}{b}");
            }

            return $"{op("/* Unknown comparison output */".WarningHighlight() + " false".ConstructHighlight())}";
        }
    }
}