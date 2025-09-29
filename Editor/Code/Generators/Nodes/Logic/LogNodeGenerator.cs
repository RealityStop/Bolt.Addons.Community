using System.Text;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(LogNode))]
    public sealed class LogNodeGenerator : NodeGenerator<LogNode>
    {
        public LogNodeGenerator(Unit unit) : base(unit) { }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var unit = Unit;
            var sb = new StringBuilder();

            string formatString = GenerateValue(unit.format, data);

            var args = new string[unit.argumentCount];
            for (int i = 0; i < unit.argumentCount; i++)
            {
                args[i] = GenerateValue(unit.arguments[i], data);
            }

            string argsJoined = args.Length > 0 ? string.Join(MakeClickableForThisUnit(", "), args) : null;
            string message;

            if (unit.argumentCount == 0)
            {
                message = formatString;
            }
            else
            {
                message = $"{MakeClickableForThisUnit("string".ConstructHighlight())}{MakeClickableForThisUnit(".Format(")}{formatString}{MakeClickableForThisUnit(", ")}{argsJoined}{MakeClickableForThisUnit(")")}";
            }

            string logCall = unit.type switch
            {
                LogType.Warning => MakeClickableForThisUnit($"{"Debug".TypeHighlight()}.LogWarning(") + message + MakeClickableForThisUnit(");"),
                LogType.Error => MakeClickableForThisUnit($"{"Debug".TypeHighlight()}.LogError(") + message + MakeClickableForThisUnit(");"),
                _ => MakeClickableForThisUnit($"{"Debug".TypeHighlight()}.Log(") + message + MakeClickableForThisUnit(");")
            };

            sb.AppendLine(CodeBuilder.Indent(indent) + logCall);
            sb.AppendLine(GetNextUnit(unit.output, data, indent));
            return sb.ToString();
        }
    }
}