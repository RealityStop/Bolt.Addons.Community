using System.Linq;
using System.Text;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(LogicParams))]
    public sealed class LogicParamsGenerator : NodeGenerator<LogicParams>
    {
        public LogicParamsGenerator(LogicParams unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (Unit.arguments.Count == 0)
                return "";

            string expr = string.Empty;

            switch (Unit.BranchingType)
            {
                case LogicParamNode.BranchType.And:
                    expr = JoinArgs(" && ", data);
                    break;
                case LogicParamNode.BranchType.Or:
                    expr = JoinArgs(" || ", data);
                    break;
                case LogicParamNode.BranchType.GreaterThan:
                    expr = CompareArgs(data, Unit.AllowEquals ? ">=" : ">");
                    break;
                case LogicParamNode.BranchType.LessThan:
                    expr = CompareArgs(data, Unit.AllowEquals ? "<=" : "<");
                    break;
                case LogicParamNode.BranchType.Equal:
                    expr = CodeBuilder.CallCSharpUtilityMethod(Unit, MakeClickableForThisUnit("Equal"), Unit.arguments.Skip(1).Select(arg => GenerateValue(arg, data)).ToArray());
                    break;
            }

            return expr;
        }

        private string JoinArgs(string op, ControlGenerationData data)
        {
            var s = "";
            s = string.Join(MakeClickableForThisUnit(op), Unit.arguments.ConvertAll(arg => GenerateValue(arg, data)));
            return s;
        }

        private string CompareArgs(ControlGenerationData data, string op)
        {
            if (Unit.arguments.Count < 2)
                return "";

            var a = GenerateValue(Unit.arguments[0], data);
            var b = GenerateValue(Unit.arguments[1], data);
            return $"{a}{MakeClickableForThisUnit(" " + op + " ")}{b}";
        }
    }
}